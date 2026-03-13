using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;

namespace Coka.Social.Listening.Core.Interfaces.Repositories;

public interface IArticleRepository : IBaseRepository<ArticleEntity>
{
    Task<PagedResult<ArticleDto>> GetArticlesAsync(ArticleFilterDto filter);
    Task<ArticleDto?> GetArticleByIdAsync(Guid id);
    Task<BulkCreateArticleResultDto> BulkCreateAsync(List<CreateArticleDto> items);
    Task<bool> UpdateArticleAsync(Guid id, UpdateArticleDto dto);
    Task<bool> DeleteArticleAsync(Guid id);
    Task<int> BulkUpdateUrlAsync(List<BulkUpdateArticleUrlItemDto> items);
    Task<bool> ExistsByUrlAsync(string url);
}
