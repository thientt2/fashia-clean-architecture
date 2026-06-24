namespace Fashia.Application.Common.Interfaces;

public interface IIdempotentRequest
{
    string IdempotencyKey { get; }

    int StoredResponseStatusCode { get; }
}

public interface IIdempotentResponse
{
    int ResourceId { get; }
}
