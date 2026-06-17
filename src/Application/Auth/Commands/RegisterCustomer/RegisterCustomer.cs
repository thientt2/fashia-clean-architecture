using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Constants;
using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;
using MediatR;

namespace Fashia.Application.Auth.Commands.RegisterCustomer;

public record RegisterCustomerCommand : IRequest<int>
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;
}

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, int>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public RegisterCustomerCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext context
    )
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<int> Handle(
        RegisterCustomerCommand request,
        CancellationToken cancellationToken
    )
    {
        // Create AspNetUser
        var (result, userId) = await _identityService.CreateUserAsync(
            request.Email,
            request.Password
        );

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Failed to create user account.");
        }

        if (!result.Succeeded)
        {
            throw new ValidationException(string.Join(", ", result.Errors));
        }

        if (await _identityService.RoleExistsAsync(Roles.Customer))
        {
            var addRoleResult = await _identityService.AddToRoleAsync(userId, Roles.Customer);

            if (!addRoleResult.Succeeded)
            {
                throw new ValidationException(string.Join(", ", addRoleResult.Errors));
            }
        }

        // Create Customer Profile
        try
        {
            var customer = Customer.Create(
                userId,
                request.FirstName,
                request.LastName,
                EmailVO.Create(request.Email),
                PhoneNumber.Create(request.PhoneNumber)
            );

            var cart = Cart.Create(customer);

            _context.Customers.Add(customer);
            _context.Carts.Add(cart);

            await _context.SaveChangesAsync(cancellationToken);
            return customer.Id;
        }
        catch
        {
            // Rollback user creation if customer profile creation fails
            await _identityService.DeleteUserAsync(userId);
            throw;
        }
    }
}
