using System.Text.Encodings.Web;
using Fashia.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using AppEmailSender = Fashia.Application.Common.Interfaces.IEmailSender;
using IdentityEmailSenderContract =
    Microsoft.AspNetCore.Identity.IEmailSender<Fashia.Infrastructure.Identity.ApplicationUser>;

namespace Fashia.Infrastructure.Email;

public sealed class IdentityEmailSender : IdentityEmailSenderContract
{
    private readonly AppEmailSender _emailSender;

    public IdentityEmailSender(AppEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task SendConfirmationLinkAsync(
        ApplicationUser user,
        string email,
        string confirmationLink)
    {
        var encodedLink = HtmlEncoder.Default.Encode(confirmationLink);

        return _emailSender.SendAsync(
            email,
            "Confirm your email",
            $"Please confirm your account by <a href='{encodedLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetLinkAsync(
        ApplicationUser user,
        string email,
        string resetLink)
    {
        var encodedLink = HtmlEncoder.Default.Encode(resetLink);

        return _emailSender.SendAsync(
            email,
            "Reset your password",
            $"Reset your password by <a href='{encodedLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetCodeAsync(
        ApplicationUser user,
        string email,
        string resetCode)
    {
        var encodedCode = HtmlEncoder.Default.Encode(resetCode);

        return _emailSender.SendAsync(
            email,
            "Reset your password",
            $"Use this code to reset your password: {encodedCode}");
    }
}
