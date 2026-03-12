using Dapper;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Interfaces;
using Coka.Social.Listening.Infra.Data;

namespace Coka.Social.Listening.Infra.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _dbFactory;

    public UserRepository(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id)
    {
        using var db = _dbFactory.CreateConnection();
        return await db.QuerySingleOrDefaultAsync<UserEntity>(
            @"SELECT id, username, email, password_hash AS PasswordHash, full_name AS FullName,
                     is_active AS IsActive, refresh_token AS RefreshToken,
                     refresh_token_expiry AS RefreshTokenExpiry,
                     created_at AS CreatedAt, updated_at AS UpdatedAt,
                     created_by AS CreatedBy, updated_by AS UpdatedBy
              FROM users WHERE id = @Id", new { Id = id });
    }

    public async Task<UserEntity?> GetByUsernameAsync(string username)
    {
        using var db = _dbFactory.CreateConnection();
        return await db.QuerySingleOrDefaultAsync<UserEntity>(
            @"SELECT id, username, email, password_hash AS PasswordHash, full_name AS FullName,
                     is_active AS IsActive, refresh_token AS RefreshToken,
                     refresh_token_expiry AS RefreshTokenExpiry,
                     created_at AS CreatedAt, updated_at AS UpdatedAt,
                     created_by AS CreatedBy, updated_by AS UpdatedBy
              FROM users WHERE username = @Username", new { Username = username });
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        using var db = _dbFactory.CreateConnection();
        return await db.QuerySingleOrDefaultAsync<UserEntity>(
            @"SELECT id, username, email, password_hash AS PasswordHash, full_name AS FullName,
                     is_active AS IsActive, refresh_token AS RefreshToken,
                     refresh_token_expiry AS RefreshTokenExpiry,
                     created_at AS CreatedAt, updated_at AS UpdatedAt,
                     created_by AS CreatedBy, updated_by AS UpdatedBy
              FROM users WHERE email = @Email", new { Email = email });
    }

    public async Task<Guid> CreateAsync(UserEntity user)
    {
        using var db = _dbFactory.CreateConnection();
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;

        await db.ExecuteAsync(
            @"INSERT INTO users (id, username, email, password_hash, full_name, is_active,
                                 created_at, updated_at, created_by, updated_by)
              VALUES (@Id, @Username, @Email, @PasswordHash, @FullName, @IsActive,
                      @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy)", user);

        return user.Id;
    }

    public async Task UpdateAsync(UserEntity user)
    {
        using var db = _dbFactory.CreateConnection();
        user.UpdatedAt = DateTime.Now;

        await db.ExecuteAsync(
            @"UPDATE users SET username = @Username, email = @Email, password_hash = @PasswordHash,
                               full_name = @FullName, is_active = @IsActive,
                               updated_at = @UpdatedAt, updated_by = @UpdatedBy
              WHERE id = @Id", user);
    }

    public async Task UpdateRefreshTokenAsync(Guid userId, string? refreshToken, DateTime? expiry)
    {
        using var db = _dbFactory.CreateConnection();
        await db.ExecuteAsync(
            @"UPDATE users SET refresh_token = @RefreshToken, refresh_token_expiry = @Expiry,
                               updated_at = @UpdatedAt
              WHERE id = @Id",
            new { Id = userId, RefreshToken = refreshToken, Expiry = expiry, UpdatedAt = DateTime.Now });
    }
}
