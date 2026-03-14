using Coka.Social.Listening.Core.Attributes;

namespace Coka.Social.Listening.Core.Entities;

[Table("users")]
public class UserEntity : BaseEntity
{
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("refresh_token")]
    public string? RefreshToken { get; set; }

    [Column("refresh_token_expiry")]
    public DateTime? RefreshTokenExpiry { get; set; }
}
