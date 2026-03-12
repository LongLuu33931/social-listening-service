namespace Coka.Social.Listening.Core.Interfaces;

public interface IRedisCacheService
{
    Task<List<string>> GetProjectNamesAsync();
    Task RefreshProjectNamesCacheAsync();
}
