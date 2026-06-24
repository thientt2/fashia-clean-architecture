using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fashia.Application.Common.Exceptions;
using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Options;
using Fashia.Domain.Entities;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Options;

namespace Fashia.Application.Common.Behaviours;

public sealed class IdempotencyBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IdempotencyOptions _options;

    public IdempotencyBehaviour(
        IApplicationDbContext context,
        IUser user,
        IOptions<IdempotencyOptions> options)
    {
        _context = context;
        _user = user;
        _options = options.Value;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not IIdempotentRequest idempotentRequest)
            return await next();

        if (request is not ITransactionalRequest)
        {
            throw new InvalidOperationException(
                "Idempotent requests must execute through the transactional pipeline."
            );
        }

        if (!IsUuidV4(idempotentRequest.IdempotencyKey))
        {
            throw new Fashia.Application.Common.Exceptions.ValidationException(
                [
                    new ValidationFailure(
                        nameof(IIdempotentRequest.IdempotencyKey),
                        "Idempotency key must be a UUID v4."
                    ),
                ]
            );
        }

        if (string.IsNullOrWhiteSpace(_user.Id))
            return await next();

        var customerId = await _context
            .Customers.Where(x => x.UserId == _user.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerId == 0)
            return await next();

        var requestHash = ComputeRequestHash(request);
        var now = DateTime.UtcNow;
        var record = IdempotencyKey.Create(
            idempotentRequest.IdempotencyKey,
            customerId,
            requestHash,
            now,
            now.Add(_options.EffectiveRetentionPeriod)
        );

        var inserted = await _context.TryInsertIdempotencyKeyAsync(record, cancellationToken);

        if (!inserted)
        {
            var existing = await _context
                .IdempotencyKeys.AsNoTracking()
                .SingleAsync(x => x.Key == idempotentRequest.IdempotencyKey, cancellationToken);

            if (existing.CustomerId != customerId || !existing.HasSameRequestHash(requestHash))
            {
                throw new IdempotencyKeyConflictException(
                    "Idempotency key has already been used with a different request."
                );
            }

            if (!existing.IsCompleted || string.IsNullOrWhiteSpace(existing.ResponseBody))
            {
                throw new DuplicateRequestInProgressException(
                    "A request with this idempotency key is already in progress."
                );
            }

            return JsonSerializer.Deserialize<TResponse>(
                    existing.ResponseBody,
                    SerializerOptions
                )
                ?? throw new InvalidOperationException("Stored idempotent response is invalid.");
        }

        var response = await next();

        if (response is not IIdempotentResponse idempotentResponse)
        {
            throw new InvalidOperationException(
                "Idempotent request responses must implement IIdempotentResponse."
            );
        }

        var responseBody = JsonSerializer.Serialize(response, SerializerOptions);

        await _context.CompleteIdempotencyKeyAsync(
            idempotentRequest.IdempotencyKey,
            idempotentResponse.ResourceId,
            idempotentRequest.StoredResponseStatusCode,
            responseBody,
            cancellationToken
        );

        return response;
    }

    private static string ComputeRequestHash(TRequest request)
    {
        var normalizedRequest = JsonSerializer.Serialize(request, SerializerOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedRequest));

        return Convert.ToHexString(hash);
    }

    private static bool IsUuidV4(string value)
    {
        return Guid.TryParse(value, out var key)
            && key.ToString("D")[14] == '4';
    }
}
