using System.ComponentModel.DataAnnotations;

namespace Coka.Social.Listening.Core.DTOs.Auth;

public class VerifyOtpRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Otp { get; set; } = string.Empty;
}
