using Coka.Social.Listening.Core.DTOs.Auth;

namespace Coka.Social.Listening.Core.Interfaces.Services;

public interface IAuthService
{
    Task<bool> SendOtpAsync(OtpRequestDto request);
    Task<AuthResponseDto?> VerifyOtpAsync(VerifyOtpRequestDto request);
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
}
