using System.ComponentModel.DataAnnotations;

namespace Coka.Social.Listening.Core.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
