using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;

namespace Coka.Social.Listening.Core.Interfaces.Repositories;

public interface IProjectRepository : IBaseRepository<ProjectEntity>
{
    Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter);
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    Task<bool> UpdateProjectAsync(Guid id, UpdateProjectDto dto);
    Task<bool> UpdateInvestorAsync(Guid id, string investor);
    Task<int> BulkUpdateInvestorAsync(List<BulkUpdateInvestorItemDto> items);
    Task<bool> UpdateConfirmationDateAsync(Guid id, DateTime confirmationDate);
    Task<int> BulkUpdateConfirmationDateAsync(List<BulkUpdateConfirmationDateItemDto> items);
    Task<bool> HideAsync(Guid id);
    Task<bool> ShowAsync(Guid id);
    Task<PagedResult<ArticleDto>> GetProjectArticlesAsync(Guid projectId, int page, int pageSize);
    Task<PagedResult<TiktokMentionDto>> GetProjectTiktokVideosAsync(Guid projectId, int page, int pageSize);
    Task<PagedResult<YoutubeMentionDto>> GetProjectYoutubeVideosAsync(Guid projectId, int page, int pageSize);
    Task<PagedResult<ProjectDto>> GetProjectsRankedByMentionsAsync(int page, int pageSize);
    Task<List<ProjectDto>> GetTopByConfirmationDateAsync(int top = 10);
}
