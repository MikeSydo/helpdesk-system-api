using System.Net;
using System.Net.Mail;
using HelpDesk.Application.Auth;
using HelpDesk.Application.Common.Interfaces;

namespace HelpDesk.API.Security;

public sealed class SmtpEmailSender(EmailOptions options) : IEmailSender
{
    public async Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentNullException.ThrowIfNull(body);

        ValidateOptions();

        using MailMessage message = new(options.Smtp.FromAddress, recipient, subject, body);
        using SmtpClient client = new(options.Smtp.Host, options.Smtp.Port);
        client.EnableSsl = options.Smtp.EnableSsl;
        client.Credentials = new NetworkCredential(
            options.Smtp.Username,
            options.Smtp.Password);

        await client.SendMailAsync(message, cancellationToken);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(options.Smtp.Host) ||
            options.Smtp.Port <= 0 ||
            string.IsNullOrWhiteSpace(options.Smtp.FromAddress) ||
            string.IsNullOrWhiteSpace(options.Smtp.Username) ||
            string.IsNullOrWhiteSpace(options.Smtp.Password))
        {
            throw new InvalidOperationException(
                "SMTP email configuration is missing. Configure it through User Secrets.");
        }

        if (string.IsNullOrWhiteSpace(options.ConfirmationLinkBaseUrl) ||
            string.IsNullOrWhiteSpace(options.PasswordResetLinkBaseUrl))
        {
            throw new InvalidOperationException(
                "Email action link configuration is missing.");
        }
    }
}
