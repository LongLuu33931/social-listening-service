namespace Coka.Social.Listening.Core.Interfaces.Services;

public interface IRedisCacheService
{
    Task<List<string>> GetProjectNamesAsync();
    Task RefreshProjectNamesCacheAsync();
}
