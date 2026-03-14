using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Coka.Social.Listening.Core.Interfaces.Services;
using Coka.Social.Listening.Core.Settings;

namespace Coka.Social.Listening.Infra.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtp;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _smtp = configuration.GetSection("Email").Get<SmtpSettings>()
            ?? throw new InvalidOperationException("SMTP settings not configured.");
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtp.DisplayName, _smtp.From));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"Mã xác thực đăng nhập của bạn là: {otp}";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; padding: 32px; background: #f9fafb; border-radius: 12px;'>
                    <h2 style='color: #1a1a2e; margin-bottom: 8px;'>Xác thực đăng nhập</h2>
                    <p style='color: #555; font-size: 14px;'>Mã OTP của bạn là:</p>
                    <div style='background: #1a1a2e; color: #fff; font-size: 32px; font-weight: bold; letter-spacing: 8px; text-align: center; padding: 16px; border-radius: 8px; margin: 16px 0;'>
                        {otp}
                    </div>
                    <p style='color: #888; font-size: 12px;'>Mã có hiệu lực trong 5 phút. Không chia sẻ mã này với bất kỳ ai.</p>
                </div>"
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            // Port 465 = implicit SSL (SslOnConnect)
            await client.ConnectAsync(_smtp.SmtpServer, _smtp.Port, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password);
            await client.SendAsync(message);
            _logger.LogInformation("OTP email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
