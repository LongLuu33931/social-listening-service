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
    /// Bulk approve/reject mentions
    /// </summary>
    [HttpPatch("bulk-approve")]
    public async Task<IActionResult> BulkApprove([FromBody] BulkApproveMentionDto dto)
    {
        var result = await _mentionService.BulkApproveAsync(dto);
        return Ok(ApiResponse<BulkApproveResultDto>.Ok(result));
    }
}
