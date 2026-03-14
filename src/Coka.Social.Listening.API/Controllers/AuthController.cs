using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.DTOs.Auth;
using Coka.Social.Listening.Core.Interfaces.Services;

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

    /// <summary>
    /// Gửi OTP về email. Nếu user chưa tồn tại sẽ tự tạo mới.
    /// </summary>
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] OtpRequestDto request)
    {
        try
        {
            var result = await _authService.SendOtpAsync(request);
            if (!result)
                return BadRequest(ApiResponse<string>.Fail("Failed to send OTP."));

            return Ok(ApiResponse<string>.Ok("OTP sent successfully.", "OTP has been sent to your email."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error sending OTP: {ex.Message}"));
        }
    }

    /// <summary>
    /// Verify OTP và trả về JWT token.
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        var result = await _authService.VerifyOtpAsync(request);
        if (result is null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid or expired OTP."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "OTP verified. Login successful."));
    }

    /// <summary>
    /// Gửi lại OTP (tạo mã mới, ghi đè mã cũ). Hiệu lực 5 phút.
    /// </summary>
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] OtpRequestDto request)
    {
        try
        {
            var result = await _authService.SendOtpAsync(request);
            if (!result)
                return BadRequest(ApiResponse<string>.Fail("Failed to resend OTP."));

            return Ok(ApiResponse<string>.Ok("OTP resent successfully.", "A new OTP has been sent to your email."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error resending OTP: {ex.Message}"));
        }
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
