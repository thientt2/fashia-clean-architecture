using Fashia.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace Fashia.Domain.UnitTests.Entities;

public class IdempotencyKeyTests
{
    [Test]
    public void ShouldCreateInFlightKey()
    {
        var createdAt = new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);

        var key = IdempotencyKey.Create(
            "7fa07c3f-5e26-45f6-8f19-8ea8f88a21dc",
            customerId: 42,
            requestHash: "hash",
            createdAt: createdAt,
            expiresAt: createdAt.AddHours(24)
        );

        key.Key.ShouldBe("7fa07c3f-5e26-45f6-8f19-8ea8f88a21dc");
        key.CustomerId.ShouldBe(42);
        key.RequestHash.ShouldBe("hash");
        key.OrderId.ShouldBeNull();
        key.ExpiresAt.ShouldBe(createdAt.AddHours(24));
        key.IsCompleted.ShouldBeFalse();
    }

    [Test]
    public void ShouldCompleteWithStoredResponse()
    {
        var key = IdempotencyKey.Create(
            "7fa07c3f-5e26-45f6-8f19-8ea8f88a21dc",
            customerId: 42,
            requestHash: "hash",
            createdAt: DateTime.UtcNow,
            expiresAt: DateTime.UtcNow.AddHours(24)
        );

        key.Complete(orderId: 99, responseStatusCode: 201, responseBody: "{\"orderId\":99}");

        key.OrderId.ShouldBe(99);
        key.ResponseStatusCode.ShouldBe(201);
        key.ResponseBody.ShouldBe("{\"orderId\":99}");
        key.IsCompleted.ShouldBeTrue();
    }

    [Test]
    public void ShouldDetectRequestHashMismatch()
    {
        var key = IdempotencyKey.Create(
            "7fa07c3f-5e26-45f6-8f19-8ea8f88a21dc",
            customerId: 42,
            requestHash: "original",
            createdAt: DateTime.UtcNow,
            expiresAt: DateTime.UtcNow.AddHours(24)
        );

        key.HasSameRequestHash("different").ShouldBeFalse();
        key.HasSameRequestHash("original").ShouldBeTrue();
    }
}
