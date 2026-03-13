using Coka.Social.Listening.Core.DTOs;

namespace Coka.Social.Listening.Core.Interfaces.Services;

public interface IProjectService
{
    Task<Guid> CreateAsync(CreateProjectDto dto);
    Task<BulkCreateProjectResultDto> BulkCreateAsync(List<CreateProjectDto> items);
    Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter);
    Task<ProjectDto?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, UpdateProjectDto dto);
    Task<List<ProjectDto>> GetTopByConfirmationDateAsync(int top);
    Task<bool> UpdateInvestorAsync(Guid id, string investor);
    Task<int> BulkUpdateInvestorAsync(List<BulkUpdateInvestorItemDto> items);
    Task<bool> UpdateConfirmationDateAsync(Guid id, DateTime date);
    Task<int> BulkUpdateConfirmationDateAsync(List<BulkUpdateConfirmationDateItemDto> items);
    Task<bool> HideAsync(Guid id);
    Task<bool> ShowAsync(Guid id);
    Task<PagedResult<ArticleDto>> GetProjectArticlesAsync(Guid projectId, int page, int pageSize);
    Task<PagedResult<TiktokMentionDto>> GetProjectTiktokVideosAsync(Guid projectId, int page, int pageSize);
    Task<PagedResult<YoutubeMentionDto>> GetProjectYoutubeVideosAsync(Guid projectId, int page, int pageSize);
    Task<PagedResult<ProjectDto>> GetProjectsRankedByMentionsAsync(int page, int pageSize);
}
