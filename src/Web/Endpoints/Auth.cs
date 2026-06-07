using System.Net.NetworkInformation;
using Fashia.Application.Auth.Commands.RegisterCustomer;
using Fashia.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints;

public class Auth : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterCustomer, "register-customer");
    }

    [EndpointSummary("Register Customer")]
    [EndpointDescription("Registers a new customer account.")]
    public static async Task<int> RegisterCustomer(ISender sender, RegisterCustomerCommand command)
    {
        return await sender.Send(command);
    }
}
