using Fashia.Application.Common.Exceptions;
using Fashia.Application.Orders.Commands.PlaceOrder;
using Fashia.Domain.Enums;

namespace Fashia.Application.FunctionalTests.Orders.Commands;

public class PlaceOrderValidationTests : TestBase
{
    [Test]
    public async Task ShouldRequireShippingAddress()
    {
        await TestApp.RunAsDefaultUserAsync();

        var command = CreateValidCommand() with { ShippingAddress = null! };

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(command)
        );

        exception.Errors.ShouldContainKey("ShippingAddress");
    }

    [Test]
    public async Task ShouldRejectInvalidCoordinates()
    {
        await TestApp.RunAsDefaultUserAsync();

        var command = CreateValidCommand() with
        {
            ShippingAddress = CreateValidAddress() with { Latitude = 91, Longitude = 181 },
        };

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(command)
        );

        exception.Errors.ShouldContainKey("ShippingAddress.Latitude");
        exception.Errors.ShouldContainKey("ShippingAddress.Longitude");
    }

    [Test]
    public async Task ShouldRejectMissingOrEmptyAuthenticatedCart()
    {
        await TestApp.RunAsDefaultUserAsync();

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(CreateValidCommand())
        );

        exception.Errors.ShouldContainKey("Cart");
    }

    private static PlaceOrderCommand CreateValidCommand()
    {
        return new PlaceOrderCommand
        {
            ShippingAddress = CreateValidAddress(),
            PaymentMethod = PaymentMethod.CashOnDelivery,
        };
    }

    private static PlaceOrderShippingAddressDto CreateValidAddress()
    {
        return new PlaceOrderShippingAddressDto
        {
            CustomerName = "Checkout Customer",
            CustomerEmail = "checkout@example.test",
            CustomerPhone = "0369405891",
            Line1 = "1 Checkout St",
            Ward = "Ward",
            District = "District",
            Province = "Province",
            Latitude = 10.1M,
            Longitude = 106.1M,
        };
    }
}
