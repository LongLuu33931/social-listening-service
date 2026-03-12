using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces;

namespace Coka.Social.Listening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// Get category tree: parent groups → children with project counts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategoryTree()
    {
        var tree = await _categoryRepository.GetCategoryTreeAsync();
        return Ok(ApiResponse<List<CategoryGroupDto>>.Ok(tree));
    }
}
