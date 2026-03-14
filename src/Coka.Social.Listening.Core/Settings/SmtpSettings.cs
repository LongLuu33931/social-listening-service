namespace Coka.Social.Listening.Core.Settings;

public class SmtpSettings
{
    public string DisplayName { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
