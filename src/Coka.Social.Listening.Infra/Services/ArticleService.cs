using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Core.Interfaces.Services;

namespace Coka.Social.Listening.Infra.Services;

public class ArticleService : IArticleService
{
    private readonly IArticleRepository _articleRepository;

    public ArticleService(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }

    public Task<PagedResult<ArticleDto>> GetArticlesAsync(ArticleFilterDto filter)
        => _articleRepository.GetArticlesAsync(filter);

    public Task<ArticleDto?> GetByIdAsync(Guid id)
        => _articleRepository.GetArticleByIdAsync(id);

    public Task<BulkCreateArticleResultDto> BulkCreateAsync(List<CreateArticleDto> items)
        => _articleRepository.BulkCreateAsync(items);

    public Task<bool> UpdateAsync(Guid id, UpdateArticleDto dto)
        => _articleRepository.UpdateArticleAsync(id, dto);

    public Task<bool> DeleteAsync(Guid id)
        => _articleRepository.DeleteArticleAsync(id);

    public Task<int> BulkUpdateUrlAsync(List<BulkUpdateArticleUrlItemDto> items)
        => _articleRepository.BulkUpdateUrlAsync(items);
}
