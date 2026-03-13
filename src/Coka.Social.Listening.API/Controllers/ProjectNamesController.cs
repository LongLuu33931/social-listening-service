using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Services;

namespace Coka.Social.Listening.API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectNamesController : ControllerBase
{
    private readonly IRedisCacheService _cacheService;

    public ProjectNamesController(IRedisCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    /// <summary>
    /// Get all project names (from Redis cache, TTL 1 hour)
    /// </summary>
    [HttpGet("names")]
    public async Task<IActionResult> GetProjectNames()
    {
        var names = await _cacheService.GetProjectNamesAsync();
        return Ok(ApiResponse<List<string>>.Ok(names, $"{names.Count} project name(s) returned"));
    }

    /// <summary>
    /// Force refresh project names cache from DB
    /// </summary>
    [HttpPost("names/refresh")]
    public async Task<IActionResult> RefreshCache()
    {
        await _cacheService.RefreshProjectNamesCacheAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Cache refreshed successfully"));
    }
}
