using Coka.Social.Listening.Core.DTOs;

namespace Coka.Social.Listening.Core.Interfaces.Services;

public interface IMentionService
{
    Task<bool> ApproveArticleAsync(Guid id, bool isApproved);
    Task<bool> ApproveTiktokAsync(int id, bool isApproved);
    Task<bool> ApproveYoutubeAsync(int id, bool isApproved);
    Task<BulkApproveResultDto> BulkApproveAsync(BulkApproveMentionDto dto);
}
