using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Services;

namespace Coka.Social.Listening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MentionsController : ControllerBase
{
    private readonly IMentionService _mentionService;

    public MentionsController(IMentionService mentionService)
    {
        _mentionService = mentionService;
    }

    /// <summary>
    /// Approve an article mention
    /// </summary>
    [HttpPatch("articles/{id:guid}/approve")]
    public async Task<IActionResult> ApproveArticle(Guid id, [FromBody] ApproveMentionDto dto)
    {
        var success = await _mentionService.ApproveArticleAsync(id, dto.IsApproved);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Article not found"));

        var msg = dto.IsApproved ? "Article approved" : "Article unapproved";
        return Ok(ApiResponse<bool>.Ok(true, msg));
    }

    /// <summary>
    /// Approve a TikTok video mention
    /// </summary>
    [HttpPatch("tiktok/{id:int}/approve")]
    public async Task<IActionResult> ApproveTiktok(int id, [FromBody] ApproveMentionDto dto)
    {
        var success = await _mentionService.ApproveTiktokAsync(id, dto.IsApproved);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("TikTok video not found"));

        var msg = dto.IsApproved ? "TikTok video approved" : "TikTok video unapproved";
        return Ok(ApiResponse<bool>.Ok(true, msg));
    }

    /// <summary>
    /// Approve a YouTube video mention
    /// </summary>
    [HttpPatch("youtube/{id:int}/approve")]
    public async Task<IActionResult> ApproveYoutube(int id, [FromBody] ApproveMentionDto dto)
    {
        var success = await _mentionService.ApproveYoutubeAsync(id, dto.IsApproved);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("YouTube video not found"));

        var msg = dto.IsApproved ? "YouTube video approved" : "YouTube video unapproved";
        return Ok(ApiResponse<bool>.Ok(true, msg));
    }

    /// <summary>
    /// Approve a Facebook post mention
    /// </summary>
    [HttpPatch("facebook/{id:int}/approve")]
    public async Task<IActionResult> ApproveFacebook(int id, [FromBody] ApproveMentionDto dto)
    {
        var success = await _mentionService.ApproveFacebookAsync(id, dto.IsApproved);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Facebook post not found"));

        var msg = dto.IsApproved ? "Facebook post approved" : "Facebook post unapproved";
        return Ok(ApiResponse<bool>.Ok(true, msg));
    }

    /// <summary>
    /// Bulk approve/reject mentions
    /// </summary>
    [HttpPatch("bulk-approve")]
    public async Task<IActionResult> BulkApprove([FromBody] BulkApproveMentionDto dto)
    {
        var result = await _mentionService.BulkApproveAsync(dto);
        return Ok(ApiResponse<BulkApproveResultDto>.Ok(result));
    }

    /// <summary>
    /// Bulk create TikTok videos. Duplicates (same video_id) are skipped.
    /// </summary>
    [HttpPost("tiktok")]
    public async Task<IActionResult> BulkCreateTiktok([FromBody] List<CreateTiktokVideoDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<BulkCreateVideoResultDto>.Fail("No TikTok videos provided"));

        var result = await _mentionService.BulkCreateTiktokAsync(items);
        return Ok(ApiResponse<BulkCreateVideoResultDto>.Ok(result));
    }

    /// <summary>
    /// Bulk create YouTube videos. Duplicates (same video_id) are skipped.
    /// </summary>
    [HttpPost("youtube")]
    public async Task<IActionResult> BulkCreateYoutube([FromBody] List<CreateYoutubeVideoDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<BulkCreateVideoResultDto>.Fail("No YouTube videos provided"));

        var result = await _mentionService.BulkCreateYoutubeAsync(items);
        return Ok(ApiResponse<BulkCreateVideoResultDto>.Ok(result));
    }

    /// <summary>
    /// Bulk create Facebook posts. Duplicates (same post_id) are skipped.
    /// </summary>
    [HttpPost("facebook")]
    public async Task<IActionResult> BulkCreateFacebook([FromBody] List<CreateFacebookPostDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<BulkCreateVideoResultDto>.Fail("No Facebook posts provided"));

        var result = await _mentionService.BulkCreateFacebookAsync(items);
        return Ok(ApiResponse<BulkCreateVideoResultDto>.Ok(result));
    }

    /// <summary>
    /// Get cached Facebook post IDs for a project (cache-aside: loads from DB on miss).
    /// </summary>
    [HttpGet("facebook/cache/{projectId:guid}")]
    public async Task<IActionResult> GetFacebookCache(Guid projectId)
    {
        var posts = await _mentionService.GetFacebookCachedPostsAsync(projectId);
        return Ok(ApiResponse<List<FacebookCachedPostDto>>.Ok(posts));
    }

    /// <summary>
    /// Force refresh the Facebook post cache for a project from DB.
    /// </summary>
    [HttpPost("facebook/cache/{projectId:guid}/refresh")]
    public async Task<IActionResult> RefreshFacebookCache(Guid projectId)
    {
        await _mentionService.RefreshFacebookCacheAsync(projectId);
        return Ok(ApiResponse<bool>.Ok(true, "Cache refreshed"));
    }
}
