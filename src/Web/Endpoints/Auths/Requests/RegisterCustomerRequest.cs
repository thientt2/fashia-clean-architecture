namespace Fashia.Web.Endpoints.Auths.Requests;

public sealed record RegisterCustomerRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber
);
