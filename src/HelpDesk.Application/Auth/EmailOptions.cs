namespace HelpDesk.Application.Auth;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public SmtpOptions Smtp { get; init; } = new();
    public string ConfirmationLinkBaseUrl { get; init; } = string.Empty;
    public string PasswordResetLinkBaseUrl { get; init; } = string.Empty;
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
}
