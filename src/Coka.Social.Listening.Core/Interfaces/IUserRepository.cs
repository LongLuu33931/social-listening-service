using Coka.Social.Listening.Core.Entities;

namespace Coka.Social.Listening.Core.Interfaces;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(Guid id);
    Task<UserEntity?> GetByUsernameAsync(string username);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<Guid> CreateAsync(UserEntity user);
    Task UpdateAsync(UserEntity user);
    Task UpdateRefreshTokenAsync(Guid userId, string? refreshToken, DateTime? expiry);
}
