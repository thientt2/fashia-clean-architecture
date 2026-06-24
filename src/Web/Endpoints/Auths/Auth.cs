using System.Net.NetworkInformation;
using Fashia.Application.Auth.Commands.RegisterCustomer;
using Fashia.Infrastructure.Identity;
using Fashia.Web.Endpoints.Auths.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints;

public class Auth : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Register, "register");
    }

    [EndpointSummary("Register")]
    [EndpointDescription("Registers a new account.")]
    public static async Task<Created<int>> Register(
        [FromServices] ISender sender,
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken
    )
    {
        var customerId = await sender.Send(
            new RegisterCustomerCommand
            {
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
            },
            cancellationToken
        );
        return TypedResults.Created($"/customers/{customerId}", customerId);
    }
}
