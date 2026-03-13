using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Services;

namespace Coka.Social.Listening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProvincesController : ControllerBase
{
    private readonly IProvinceService _provinceService;

    public ProvincesController(IProvinceService provinceService)
    {
        _provinceService = provinceService;
    }

    /// <summary>
    /// Get all provinces. Optionally filter by region (bac/trung/nam) or search by name.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProvinces(
        [FromQuery] string? search,
        [FromQuery] string? region)
    {
        var filter = new ProvinceFilterDto { Search = search, Region = region };
        var result = await _provinceService.GetAllAsync(filter);
        return Ok(ApiResponse<List<ProvinceDto>>.Ok(result));
    }

    /// <summary>
    /// Get a province by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var province = await _provinceService.GetByIdAsync(id);
        if (province is null)
            return NotFound(ApiResponse<ProvinceDto>.Fail("Province not found"));

        return Ok(ApiResponse<ProvinceDto>.Ok(province));
    }

    /// <summary>
    /// Get a province by slug (e.g. "ha-noi", "ho-chi-minh").
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var province = await _provinceService.GetBySlugAsync(slug);
        if (province is null)
            return NotFound(ApiResponse<ProvinceDto>.Fail("Province not found"));

        return Ok(ApiResponse<ProvinceDto>.Ok(province));
    }
}
