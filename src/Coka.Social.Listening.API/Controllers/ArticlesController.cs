using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Services;

namespace Coka.Social.Listening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    /// <summary>
    /// Get articles with pagination and filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetArticles(
        [FromQuery] string? search,
        [FromQuery] Guid? projectId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? sentiment,
        [FromQuery] bool? isRelevant,
        [FromQuery] string? urlContains,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var filter = new ArticleFilterDto
        {
            Search = search,
            ProjectId = projectId,
            FromDate = fromDate,
            ToDate = toDate,
            Sentiment = sentiment,
            IsRelevant = isRelevant,
            UrlContains = urlContains,
            Page = page,
            PageSize = pageSize
        };

        var result = await _articleService.GetArticlesAsync(filter);
        return Ok(ApiResponse<PagedResult<ArticleDto>>.Ok(result));
    }

    /// <summary>
    /// Get a single article by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var article = await _articleService.GetByIdAsync(id);
        if (article is null)
            return NotFound(ApiResponse<ArticleDto>.Fail("Article not found"));

        return Ok(ApiResponse<ArticleDto>.Ok(article));
    }

    /// <summary>
    /// Bulk create articles. Duplicates (same URL) are skipped.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> BulkCreate([FromBody] List<CreateArticleDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<BulkCreateArticleResultDto>.Fail("No articles provided"));

        var result = await _articleService.BulkCreateAsync(items);
        return Ok(ApiResponse<BulkCreateArticleResultDto>.Ok(result));
    }

    /// <summary>
    /// Update an article (partial update — only provided fields are changed).
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleDto dto)
    {
        var success = await _articleService.UpdateAsync(id, dto);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Article not found"));

        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>
    /// Soft-delete an article (sets status = 0).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _articleService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("Article not found"));

        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>
    /// Bulk update article URLs.
    /// </summary>
    [HttpPatch("bulk-update-urls")]
    public async Task<IActionResult> BulkUpdateUrls([FromBody] List<BulkUpdateArticleUrlItemDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<int>.Fail("No items provided"));

        var updated = await _articleService.BulkUpdateUrlAsync(items);
        return Ok(ApiResponse<int>.Ok(updated, $"{updated} article(s) updated"));
    }
}
