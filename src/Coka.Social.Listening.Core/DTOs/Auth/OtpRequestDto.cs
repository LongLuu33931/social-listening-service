using System.ComponentModel.DataAnnotations;

namespace Coka.Social.Listening.Core.DTOs.Auth;

public class OtpRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
