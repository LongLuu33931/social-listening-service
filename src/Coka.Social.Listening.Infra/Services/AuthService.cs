using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Coka.Social.Listening.Core.DTOs.Auth;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Core.Interfaces.Services;
using Coka.Social.Listening.Core.Settings;
using Coka.Social.Listening.Infra.Helpers;

namespace Coka.Social.Listening.Infra.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly RedisHelper _redis;
    private readonly JwtSettings _jwtSettings;

    private const int OtpLength = 6;
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);

    public AuthService(
        IUserRepository userRepository,
        IEmailService emailService,
        RedisHelper redis,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _redis = redis;
        _jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings not configured.");
    }

    // ─── OTP Flow ──────────────────────────────────────────────────────────

    public async Task<bool> SendOtpAsync(OtpRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Check if user exists, if not create one
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            user = new UserEntity
            {
                Username = email,
                Email = email,
                IsActive = true
            };
            await _userRepository.CreateAsync(user);
        }

        // Generate OTP
        var otp = GenerateOtp();

        // Store OTP in Redis with TTL
        var redisKey = $"otp:{email}";
        await _redis.SetStringAsync(redisKey, otp, OtpTtl);

        // Send OTP email (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try { await _emailService.SendOtpEmailAsync(email, otp); }
            catch { /* log if needed */ }
        });

        return true;
    }

    public async Task<AuthResponseDto?> VerifyOtpAsync(VerifyOtpRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var redisKey = $"otp:{email}";

        // Get stored OTP from Redis
        var storedOtp = await _redis.GetStringAsync(redisKey);
        if (storedOtp is null || storedOtp != request.Otp)
            return null;

        // OTP verified — delete it
        await _redis.DeleteAsync(redisKey);

        // Find user and generate tokens
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null || !user.IsActive)
            return null;

        return await GenerateTokens(user);
    }

    // ─── Refresh Token ─────────────────────────────────────────────────────

    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal is null)
            return null;

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null || !Guid.TryParse(userId, out var id))
            return null;

        var user = await _userRepository.GetByIdAsync(id);
        if (user is null || user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiry <= DateTime.Now)
            return null;

        return await GenerateTokens(user);
    }

    // ─── Helpers ───────────────────────────────────────────────────────────

    private static string GenerateOtp()
    {
        var bytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % (int)Math.Pow(10, OtpLength);
        return number.ToString().PadLeft(OtpLength, '0');
    }

    private async Task<AuthResponseDto> GenerateTokens(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.Now.AddMinutes(_jwtSettings.TokenValidityInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.ValidIssuer,
            audience: _jwtSettings.ValidAudience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        var refreshToken = GenerateRefreshToken();
        var refreshExpiry = DateTime.Now.AddDays(_jwtSettings.RefreshTokenValidityInDays);

        await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken, refreshExpiry);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Expiration = expiration
        };
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidAudience = _jwtSettings.ValidAudience,
            ValidIssuer = _jwtSettings.ValidIssuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false // allow expired tokens for refresh
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
