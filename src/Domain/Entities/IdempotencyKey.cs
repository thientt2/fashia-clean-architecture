namespace Fashia.Domain.Entities;

public sealed class IdempotencyKey : BaseEntity
{
    private IdempotencyKey()
    {
        // EF Core
    }

    private IdempotencyKey(
        string key,
        int customerId,
        string requestHash,
        DateTime createdAt,
        DateTime expiresAt
    )
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key is required.", nameof(key));

        if (customerId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        if (string.IsNullOrWhiteSpace(requestHash))
            throw new ArgumentException("Request hash is required.", nameof(requestHash));

        if (expiresAt <= createdAt)
            throw new ArgumentException("Expiry must be after creation time.", nameof(expiresAt));

        Key = key.Trim();
        CustomerId = customerId;
        RequestHash = requestHash.Trim();
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public string Key { get; private set; } = string.Empty;

    public int CustomerId { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public string RequestHash { get; private set; } = string.Empty;

    public int? OrderId { get; private set; }

    public Order? Order { get; private set; }

    public int? ResponseStatusCode { get; private set; }

    public string? ResponseBody { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public bool IsCompleted => OrderId.HasValue;

    public static IdempotencyKey Create(
        string key,
        int customerId,
        string requestHash,
        DateTime createdAt,
        DateTime expiresAt
    )
    {
        return new IdempotencyKey(key, customerId, requestHash, createdAt, expiresAt);
    }

    public bool HasSameRequestHash(string requestHash)
    {
        return RequestHash == requestHash;
    }

    public void Complete(int orderId, int responseStatusCode, string responseBody)
    {
        if (orderId <= 0)
            throw new ArgumentException("Order id is required.", nameof(orderId));

        if (responseStatusCode < 100 || responseStatusCode > 599)
            throw new ArgumentOutOfRangeException(
                nameof(responseStatusCode),
                "Response status code must be a valid HTTP status code."
            );

        if (string.IsNullOrWhiteSpace(responseBody))
            throw new ArgumentException("Response body is required.", nameof(responseBody));

        OrderId = orderId;
        ResponseStatusCode = responseStatusCode;
        ResponseBody = responseBody;
    }
}
