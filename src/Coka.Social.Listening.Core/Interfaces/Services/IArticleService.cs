using Coka.Social.Listening.Core.DTOs;

namespace Coka.Social.Listening.Core.Interfaces.Services;

public interface IArticleService
{
    Task<PagedResult<ArticleDto>> GetArticlesAsync(ArticleFilterDto filter);
    Task<ArticleDto?> GetByIdAsync(Guid id);
    Task<BulkCreateArticleResultDto> BulkCreateAsync(List<CreateArticleDto> items);
    Task<bool> UpdateAsync(Guid id, UpdateArticleDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<int> BulkUpdateUrlAsync(List<BulkUpdateArticleUrlItemDto> items);
}
