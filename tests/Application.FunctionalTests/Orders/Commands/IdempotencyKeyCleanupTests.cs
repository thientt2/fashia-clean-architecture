using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;
using Fashia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.FunctionalTests.Orders.Commands;

public class IdempotencyKeyCleanupTests : TestBase
{
    [Test]
    public async Task ShouldDeleteExpiredKeysAndKeepActiveKeys()
    {
        var now = new DateTime(2026, 6, 23, 8, 0, 0, DateTimeKind.Utc);
        var userId = await TestApp.RunAsUserAsync(
            $"{Guid.NewGuid():N}@local.test",
            "Testing1234!",
            []
        );
        var customer = Customer.Create(
            userId,
            firstName: "Checkout",
            lastName: "Customer",
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            PhoneNumber.Create("0369405891")
        );

        await TestApp.ExecuteDbContextAsync(async context =>
        {
            context.Customers.Add(customer);
            await context.SaveChangesAsync(CancellationToken.None);

            var expired = IdempotencyKey.Create(
                "7fa07c3f-5e26-45f6-8f19-8ea8f88a21dc",
                customer.Id,
                "expired",
                now.AddHours(-25),
                now.AddMinutes(-1)
            );
            var active = IdempotencyKey.Create(
                "018c654a-cfe0-40c7-a0d2-983399db842a",
                customer.Id,
                "active",
                now.AddHours(-1),
                now.AddHours(23)
            );

            context.IdempotencyKeys.AddRange(expired, active);
            await context.SaveChangesAsync(CancellationToken.None);
            return 0;
        });

        var deleted = await TestApp.ExecuteDbContextAsync(context =>
            IdempotencyKeyCleanupService.DeleteExpiredKeysAsync(
                context,
                now,
                CancellationToken.None
            )
        );

        deleted.ShouldBe(1);

        var remainingKeys = await TestApp.ExecuteDbContextAsync(context =>
            context
                .IdempotencyKeys.AsNoTracking()
                .OrderBy(x => x.Key)
                .Select(x => x.Key)
                .ToListAsync()
        );

        remainingKeys.ShouldBe(["018c654a-cfe0-40c7-a0d2-983399db842a"]);
    }
}
