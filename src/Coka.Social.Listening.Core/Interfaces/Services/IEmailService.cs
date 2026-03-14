namespace Coka.Social.Listening.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otp);
}
