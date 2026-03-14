using Coka.Social.Listening.Core.DTOs;

namespace Coka.Social.Listening.Core.Interfaces.Services;

public interface IMentionService
{
    Task<bool> ApproveArticleAsync(Guid id, bool isApproved);
    Task<bool> ApproveTiktokAsync(int id, bool isApproved);
    Task<bool> ApproveYoutubeAsync(int id, bool isApproved);
    Task<bool> ApproveFacebookAsync(int id, bool isApproved);
    Task<BulkApproveResultDto> BulkApproveAsync(BulkApproveMentionDto dto);
    Task<BulkCreateVideoResultDto> BulkCreateTiktokAsync(List<CreateTiktokVideoDto> items);
    Task<BulkCreateVideoResultDto> BulkCreateYoutubeAsync(List<CreateYoutubeVideoDto> items);
    Task<BulkCreateVideoResultDto> BulkCreateFacebookAsync(List<CreateFacebookPostDto> items);
    Task<List<FacebookCachedPostDto>> GetFacebookCachedPostsAsync(Guid projectId);
    Task RefreshFacebookCacheAsync(Guid projectId);
}
