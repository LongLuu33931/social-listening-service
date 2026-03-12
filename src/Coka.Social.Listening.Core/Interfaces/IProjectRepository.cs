using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;

namespace Coka.Social.Listening.Core.Interfaces;

public interface IProjectRepository : IBaseRepository<ProjectEntity>
{
    Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter);
    Task<bool> UpdateInvestorAsync(Guid id, string investor);
    Task<int> BulkUpdateInvestorAsync(List<BulkUpdateInvestorItemDto> items);
    Task<bool> UpdateConfirmationDateAsync(Guid id, DateOnly confirmationDate);
    Task<int> BulkUpdateConfirmationDateAsync(List<BulkUpdateConfirmationDateItemDto> items);
    Task<bool> HideAsync(Guid id);
    Task<List<ProjectDto>> GetTopByConfirmationDateAsync(int top = 10);
}
