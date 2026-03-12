namespace Coka.Social.Listening.Core.Settings;

public class JwtSettings
{
    public string ValidAudience { get; set; } = string.Empty;
    public string ValidIssuer { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public int TokenValidityInMinutes { get; set; }
    public int RefreshTokenValidityInDays { get; set; }
}
