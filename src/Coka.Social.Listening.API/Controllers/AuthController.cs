using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.DTOs.Auth;
using Coka.Social.Listening.Core.Interfaces;

namespace Coka.Social.Listening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid username or password."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result is null)
            return BadRequest(ApiResponse<string>.Fail("Username or email already exists."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration successful."));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (result is null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid or expired token."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token refreshed."));
    }
}
