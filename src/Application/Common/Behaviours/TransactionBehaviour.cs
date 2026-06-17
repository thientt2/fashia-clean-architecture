using Fashia.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fashia.Application.Common.Behaviours;

public sealed class TransactionBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;

    public TransactionBehaviour(
        IApplicationDbContext context,
        ILogger<TransactionBehaviour<TRequest, TResponse>> logger
    )
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not ITransactionalRequest)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Beginning transaction for request {RequestName}", requestName);

            var response = await next();

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Committed transaction for request {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rolling back transaction for request {RequestName}", requestName);

            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }
}
