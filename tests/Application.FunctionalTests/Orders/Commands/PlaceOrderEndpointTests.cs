using System.Net;
using System.Net.Http.Json;
using Fashia.Application.Common.Exceptions;
using Fashia.Application.Orders.Commands.PlaceOrder;
using Fashia.Domain.Enums;
using Fashia.Web.Endpoints.Orders;
using Fashia.Web.Endpoints.Orders.Requests;
using MediatR;
using OrdersEndpoint = Fashia.Web.Endpoints.Orders.Orders;
using Fashia.Web.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Application.FunctionalTests.Orders.Commands;

public class PlaceOrderEndpointTests : TestBase
{
    [Test]
    public async Task PlaceOrderEndpointShouldRequireAuthentication()
    {
        using var client = TestApp.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders/placeorder", CreateRequest());

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PlaceOrderEndpointShouldRejectMissingIdempotencyKeyWithValidationErrors()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var client = TestApp.CreateClient();
        await LoginAsync(client);

        var response = await client.PostAsJsonAsync("/api/orders/placeorder", CreateRequest());

        var responseBody = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, responseBody);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.ShouldNotBeNull();
        problem!.Errors.ShouldContainKey("IdempotencyKey");
    }

    [Test]
    public async Task OldCheckoutEndpointShouldNotBeExposed()
    {
        using var client = TestApp.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders/checkout", CreateRequest());

        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task PlaceOrderEndpointShouldSendCommandAndReturnCreatedResponse()
    {
        var key = Guid.NewGuid().ToString();
        var expected = new OrderCreated
        {
            OrderId = 42,
            Status = OrderStatus.Pending,
            BranchId = 7,
            BranchName = "Central Branch",
        };
        PlaceOrderCommand? sentCommand = null;
        var sender = new Mock<ISender>();
        sender
            .Setup(x => x.Send(It.IsAny<PlaceOrderCommand>(), CancellationToken.None))
            .Callback<IRequest<OrderCreated>, CancellationToken>((request, _) =>
                sentCommand = (PlaceOrderCommand)request
            )
            .ReturnsAsync(expected);
        var request = new PlaceOrderRequest
        {
            ShippingAddress = new PlaceOrderShippingAddressRequest
            {
                CustomerName = "Test Customer",
                CustomerEmail = "test@local",
                CustomerPhone = "0369405891",
                Line1 = "1 Checkout Street",
                Ward = "Ward 1",
                District = "District 1",
                Province = "Ho Chi Minh City",
                Latitude = 10.7769M,
                Longitude = 106.7009M,
            },
            VoucherCode = "SAVE10",
            PaymentMethod = PaymentMethod.CashOnDelivery,
            Note = "Leave at reception",
        };

        var response = await OrdersEndpoint.PlaceOrder(
            sender.Object,
            key,
            request,
            CancellationToken.None
        );

        response.StatusCode.ShouldBe(StatusCodes.Status201Created);
        response.Location.ShouldBe("/api/orders/42");
        response.Value.ShouldBe(expected);
        sentCommand.ShouldNotBeNull();
        sentCommand!.IdempotencyKey.ShouldBe(key);
        sentCommand.VoucherCode.ShouldBe(request.VoucherCode);
        sentCommand.ShippingAddress.Latitude.ShouldBe(request.ShippingAddress.Latitude);
    }

    [TestCase(typeof(IdempotencyKeyConflictException), StatusCodes.Status409Conflict)]
    [TestCase(typeof(DuplicateRequestInProgressException), StatusCodes.Status409Conflict)]
    [TestCase(typeof(InvalidVoucherException), StatusCodes.Status422UnprocessableEntity)]
    [TestCase(typeof(InsufficientStockException), StatusCodes.Status409Conflict)]
    public async Task CheckoutBusinessExceptionsShouldMapToProblemDetails(
        Type exceptionType,
        int expectedStatusCode
    )
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Checkout failed.")!;
        var problemDetailsService = new Mock<IProblemDetailsService>();
        problemDetailsService
            .Setup(x => x.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Returns(new ValueTask<bool>(true));
        var handler = new ProblemDetailsExceptionHandler(problemDetailsService.Object);
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
    }

    private static async Task LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/users/login?useCookies=true",
            new { Email = "test@local", Password = "Testing1234!" }
        );

        response.EnsureSuccessStatusCode();
    }

    private static object CreateRequest() =>
        new
        {
            ShippingAddress = new
            {
                CustomerName = "Test Customer",
                CustomerEmail = "test@local",
                CustomerPhone = "0369405891",
                Line1 = "1 Checkout Street",
                Ward = "Ward 1",
                District = "District 1",
                Province = "Ho Chi Minh City",
                Latitude = 10.7769M,
                Longitude = 106.7009M,
            },
            PaymentMethod = 0,
        };
}
