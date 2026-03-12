using Coka.Social.Listening.Core.DTOs.Auth;

namespace Coka.Social.Listening.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
}
