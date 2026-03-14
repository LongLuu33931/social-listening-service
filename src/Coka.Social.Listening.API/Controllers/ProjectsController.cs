using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Services;

namespace Coka.Social.Listening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto request)
    {
        var id = await _projectService.CreateAsync(request);
        return Ok(ApiResponse<Guid>.Ok(id, "Project created successfully"));
    }

    /// <summary>
    /// Bulk create multiple projects (with optional articles per project).
    /// Articles are deduplicated by URL — duplicates are skipped automatically.
    /// </summary>
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreateProjects([FromBody] List<CreateProjectDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<int>.Fail("Items list cannot be empty"));

        var result = await _projectService.BulkCreateAsync(items);

        var message = $"{result.ProjectIds.Count} project(s) created";
        if (result.ArticlesInserted > 0 || result.ArticlesSkipped > 0)
            message += $", {result.ArticlesInserted} article(s) inserted, {result.ArticlesSkipped} skipped (duplicate URL)";

        return Ok(ApiResponse<BulkCreateProjectResultDto>.Ok(result, message));
    }

    /// <summary>
    /// Get projects with pagination, filter by category/group, search by name/investor
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProjects(
        [FromQuery] string? search,
        [FromQuery] string? categoryKey,
        [FromQuery] string? groupKey,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        // Detect admin context from Referer or Origin header
        var referer = Request.Headers.Referer.ToString();
        var origin = Request.Headers.Origin.ToString();
        var isAdmin = referer.Contains("admin", StringComparison.OrdinalIgnoreCase)
                   || origin.Contains("admin", StringComparison.OrdinalIgnoreCase);

        var filter = new ProjectFilterDto
        {
            Search = search,
            CategoryKey = categoryKey,
            GroupKey = groupKey,
            Page = page,
            PageSize = pageSize,
            IsAdmin = isAdmin
        };

        var result = await _projectService.GetProjectsAsync(filter);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Get public projects (only active, only approved mentions counted)
    /// </summary>
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicProjects(
        [FromQuery] string? search,
        [FromQuery] string? categoryKey,
        [FromQuery] string? groupKey,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var filter = new ProjectFilterDto
        {
            Search = search,
            CategoryKey = categoryKey,
            GroupKey = groupKey,
            Page = page,
            PageSize = pageSize,
            IsAdmin = false
        };

        var result = await _projectService.GetProjectsAsync(filter);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Get top N projects sorted by confirmation_date descending (newest first)
    /// </summary>
    [HttpGet("top-confirmed")]
    public async Task<IActionResult> GetTopConfirmed([FromQuery] int top = 10)
    {
        if (top < 1) top = 10;
        if (top > 100) top = 100;

        var result = await _projectService.GetTopByConfirmationDateAsync(top);
        return Ok(ApiResponse<List<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Get projects with missing or empty investor
    /// </summary>
    [HttpGet("missing-investors")]
    public async Task<IActionResult> GetMissingInvestors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var filter = new ProjectFilterDto { MissingInvestor = true, Page = page, PageSize = pageSize };
        var result = await _projectService.GetProjectsAsync(filter);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Get projects with missing confirmation date
    /// </summary>
    [HttpGet("missing-confirmation-date")]
    public async Task<IActionResult> GetMissingConfirmationDate([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var filter = new ProjectFilterDto { MissingConfirmationDate = true, Page = page, PageSize = pageSize };
        var result = await _projectService.GetProjectsAsync(filter);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Update project investor
    /// </summary>
    [HttpPatch("{id}/investor")]
    public async Task<IActionResult> UpdateInvestor(Guid id, [FromBody] UpdateInvestorDto request)
    {
        var success = await _projectService.UpdateInvestorAsync(id, request.Investor);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Project not found"));
            
        return Ok(ApiResponse<bool>.Ok(true, "Investor updated successfully"));
    }

    /// <summary>
    /// Bulk update project investors
    /// </summary>
    [HttpPatch("bulk-update-investors")]
    public async Task<IActionResult> BulkUpdateInvestors([FromBody] List<BulkUpdateInvestorItemDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<int>.Fail("Items list cannot be empty"));

        var affected = await _projectService.BulkUpdateInvestorAsync(items);
        return Ok(ApiResponse<int>.Ok(affected, $"{affected} project(s) updated successfully"));
    }

    /// <summary>
    /// Update project confirmation date
    /// </summary>
    [HttpPatch("{id}/confirmation-date")]
    public async Task<IActionResult> UpdateConfirmationDate(Guid id, [FromBody] UpdateConfirmationDateDto request)
    {
        var success = await _projectService.UpdateConfirmationDateAsync(id, request.ConfirmationDate);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Project not found"));
            
        return Ok(ApiResponse<bool>.Ok(true, "Confirmation date updated successfully"));
    }

    /// <summary>
    /// Bulk update project confirmation dates
    /// </summary>
    [HttpPatch("bulk-update-confirmation-dates")]
    public async Task<IActionResult> BulkUpdateConfirmationDates([FromBody] List<BulkUpdateConfirmationDateItemDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<int>.Fail("Items list cannot be empty"));

        var affected = await _projectService.BulkUpdateConfirmationDateAsync(items);
        return Ok(ApiResponse<int>.Ok(affected, $"{affected} project(s) updated successfully"));
    }

    /// <summary>
    /// Get projects ranked by total mentions (articles + tiktok + youtube + facebook) descending.
    /// Use period=week for last 7 days, period=month for last 30 days, or omit for all time.
    /// </summary>
    [HttpGet("ranking")]
    public async Task<IActionResult> GetProjectsRanking(
        [FromQuery] string? period,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await _projectService.GetProjectsRankedByMentionsAsync(page, pageSize, period);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Get project detail by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project is null)
            return NotFound(ApiResponse<ProjectDto>.Fail("Project not found"));

        return Ok(ApiResponse<ProjectDto>.Ok(project));
    }

    /// <summary>
    /// Partial update project — only fields provided will be updated.
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var success = await _projectService.UpdateAsync(id, dto);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Project not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Project updated successfully"));
    }

    /// <summary>
    /// Hide a project (change status to 2)
    /// </summary>
    [HttpPatch("{id}/hide")]
    public async Task<IActionResult> HideProject(Guid id)
    {
        var success = await _projectService.HideAsync(id);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Project not found"));
            
        return Ok(ApiResponse<bool>.Ok(true, "Project hidden successfully"));
    }

    /// <summary>
    /// Show a hidden project (change status back to 1)
    /// </summary>
    [HttpPatch("{id}/show")]
    public async Task<IActionResult> ShowProject(Guid id)
    {
        var success = await _projectService.ShowAsync(id);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Project not found or not hidden"));
            
        return Ok(ApiResponse<bool>.Ok(true, "Project shown successfully"));
    }

    /// <summary>
    /// Get articles (mentions) for a project
    /// </summary>
    [HttpGet("{id:guid}/mentions/articles")]
    public async Task<IActionResult> GetProjectArticles(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await _projectService.GetProjectArticlesAsync(id, page, pageSize, IsAdminRequest());
        return Ok(ApiResponse<PagedResult<ArticleDto>>.Ok(result));
    }

    /// <summary>
    /// Get TikTok videos (mentions) for a project
    /// </summary>
    [HttpGet("{id:guid}/mentions/tiktok")]
    public async Task<IActionResult> GetProjectTiktokVideos(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await _projectService.GetProjectTiktokVideosAsync(id, page, pageSize, IsAdminRequest());
        return Ok(ApiResponse<PagedResult<TiktokMentionDto>>.Ok(result));
    }

    /// <summary>
    /// Get YouTube videos (mentions) for a project
    /// </summary>
    [HttpGet("{id:guid}/mentions/youtube")]
    public async Task<IActionResult> GetProjectYoutubeVideos(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await _projectService.GetProjectYoutubeVideosAsync(id, page, pageSize, IsAdminRequest());
        return Ok(ApiResponse<PagedResult<YoutubeMentionDto>>.Ok(result));
    }

    /// <summary>
    /// Get Facebook posts (mentions) for a project
    /// </summary>
    [HttpGet("{id:guid}/mentions/facebook")]
    public async Task<IActionResult> GetProjectFacebookPosts(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await _projectService.GetProjectFacebookPostsAsync(id, page, pageSize, IsAdminRequest());
        return Ok(ApiResponse<PagedResult<FacebookPostMentionDto>>.Ok(result));
    }

    /// <summary>
    /// Client submits a new project for review (status = 0, pending approval)
    /// </summary>
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitProject([FromBody] SubmitProjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(ApiResponse<Guid>.Fail("Project name is required"));

        var id = await _projectService.SubmitAsync(dto);
        return Ok(ApiResponse<Guid>.Ok(id, "Project submitted successfully, pending approval"));
    }

    /// <summary>
    /// Get pending project submissions (status = 0)
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await _projectService.GetPendingProjectsAsync(page, pageSize);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Approve a pending project submission (status 0 → 1)
    /// </summary>
    [HttpPatch("{id:guid}/approve-submission")]
    public async Task<IActionResult> ApproveSubmission(Guid id)
    {
        var success = await _projectService.ApproveSubmissionAsync(id);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Pending project not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Project approved successfully"));
    }

    /// <summary>
    /// Reject (delete) a pending project submission
    /// </summary>
    [HttpDelete("{id:guid}/reject-submission")]
    public async Task<IActionResult> RejectSubmission(Guid id)
    {
        var success = await _projectService.RejectSubmissionAsync(id);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Pending project not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Project rejected and removed"));
    }

    private bool IsAdminRequest()
    {
        var referer = Request.Headers.Referer.ToString();
        var origin = Request.Headers.Origin.ToString();
        return referer.Contains("admin", StringComparison.OrdinalIgnoreCase)
            || origin.Contains("admin", StringComparison.OrdinalIgnoreCase);
    }
}

public class UpdateInvestorDto
{
    public string Investor { get; set; } = string.Empty;
}

public class UpdateConfirmationDateDto
{
    public DateTime ConfirmationDate { get; set; }
}
