using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Interfaces;

namespace Coka.Social.Listening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IRedisCacheService _cacheService;

    public ProjectsController(IProjectRepository projectRepository, IRedisCacheService cacheService)
    {
        _projectRepository = projectRepository;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto request)
    {
        var entity = new ProjectEntity
        {
            Name = request.Name,
            Source = request.Source,
            Location = request.Location,
            Investor = request.Investor,
            OriginUrl = request.OriginUrl,
            Logo = request.Logo,
            CategoryKey = request.CategoryKey,
            ConfirmationDate = request.ConfirmationDate
        };

        var id = await _projectRepository.AddAsync(entity);
        await _cacheService.RefreshProjectNamesCacheAsync();
        return Ok(ApiResponse<Guid>.Ok(id, "Project created successfully"));
    }

    /// <summary>
    /// Bulk create multiple projects
    /// </summary>
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreateProjects([FromBody] List<CreateProjectDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<int>.Fail("Items list cannot be empty"));

        var ids = new List<Guid>();
        foreach (var request in items)
        {
            var entity = new ProjectEntity
            {
                Name = request.Name,
                Source = request.Source,
                Location = request.Location,
                Investor = request.Investor,
                OriginUrl = request.OriginUrl,
                Logo = request.Logo,
                CategoryKey = request.CategoryKey,
                ConfirmationDate = request.ConfirmationDate
            };
            var id = await _projectRepository.AddAsync(entity);
            ids.Add(id);
        }

        await _cacheService.RefreshProjectNamesCacheAsync();
        return Ok(ApiResponse<List<Guid>>.Ok(ids, $"{ids.Count} project(s) created successfully"));
    }

    /// <summary>
    /// Get projects with pagination, filter by category/group, search by name/investor
    /// </summary>
    /// <param name="search">Search by project name or investor name</param>
    /// <param name="categoryKey">Filter by specific category key (e.g. "apartment")</param>
    /// <param name="groupKey">Filter by category group (e.g. "residential")</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
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

        var filter = new ProjectFilterDto
        {
            Search = search,
            CategoryKey = categoryKey,
            GroupKey = groupKey,
            Page = page,
            PageSize = pageSize
        };

        var result = await _projectRepository.GetProjectsAsync(filter);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Get top N projects sorted by confirmation_date descending (newest first)
    /// </summary>
    /// <param name="top">Number of results (default: 10)</param>
    [HttpGet("top-confirmed")]
    public async Task<IActionResult> GetTopConfirmed([FromQuery] int top = 10)
    {
        if (top < 1) top = 10;
        if (top > 100) top = 100;

        var result = await _projectRepository.GetTopByConfirmationDateAsync(top);
        return Ok(ApiResponse<List<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Get projects with missing or empty investor ("" or "Đang cập nhật")
    /// </summary>
    [HttpGet("missing-investors")]
    public async Task<IActionResult> GetMissingInvestors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var filter = new ProjectFilterDto { MissingInvestor = true, Page = page, PageSize = pageSize };
        var result = await _projectRepository.GetProjectsAsync(filter);
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
        if (pageSize > 100) pageSize = 100;

        var filter = new ProjectFilterDto { MissingConfirmationDate = true, Page = page, PageSize = pageSize };
        var result = await _projectRepository.GetProjectsAsync(filter);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Update project investor
    /// </summary>
    [HttpPatch("{id}/investor")]
    public async Task<IActionResult> UpdateInvestor(Guid id, [FromBody] UpdateInvestorDto request)
    {
        var success = await _projectRepository.UpdateInvestorAsync(id, request.Investor);
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

        var affected = await _projectRepository.BulkUpdateInvestorAsync(items);
        return Ok(ApiResponse<int>.Ok(affected, $"{affected} project(s) updated successfully"));
    }

    /// <summary>
    /// Update project confirmation date
    /// </summary>
    [HttpPatch("{id}/confirmation-date")]
    public async Task<IActionResult> UpdateConfirmationDate(Guid id, [FromBody] UpdateConfirmationDateDto request)
    {
        var success = await _projectRepository.UpdateConfirmationDateAsync(id, request.ConfirmationDate);
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

        var affected = await _projectRepository.BulkUpdateConfirmationDateAsync(items);
        return Ok(ApiResponse<int>.Ok(affected, $"{affected} project(s) updated successfully"));
    }

    /// <summary>
    /// Hide a project (change status to 2)
    /// </summary>
    [HttpPatch("{id}/hide")]
    public async Task<IActionResult> HideProject(Guid id)
    {
        var success = await _projectRepository.HideAsync(id);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Project not found"));
            
        return Ok(ApiResponse<bool>.Ok(true, "Project hidden successfully"));
    }
}

public class UpdateInvestorDto
{
    public string Investor { get; set; } = string.Empty;
}

public class UpdateConfirmationDateDto
{
    public DateOnly ConfirmationDate { get; set; }
}
