using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Core.Interfaces.Services;

namespace Coka.Social.Listening.Infra.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IArticleRepository _articleRepository;
    private readonly IRedisCacheService _cacheService;

    public ProjectService(
        IProjectRepository projectRepository,
        IArticleRepository articleRepository,
        IRedisCacheService cacheService)
    {
        _projectRepository = projectRepository;
        _articleRepository = articleRepository;
        _cacheService = cacheService;
    }

    public async Task<Guid> CreateAsync(CreateProjectDto dto)
    {
        var entity = MapToEntity(dto);
        var id = await _projectRepository.AddAsync(entity);

        // Handle articles if provided
        if (dto.Articles != null && dto.Articles.Count > 0)
        {
            foreach (var article in dto.Articles)
                article.ProjectId = id;

            await _articleRepository.BulkCreateAsync(dto.Articles);
        }

        await _cacheService.RefreshProjectNamesCacheAsync();
        return id;
    }

    public async Task<BulkCreateProjectResultDto> BulkCreateAsync(List<CreateProjectDto> items)
    {
        var result = new BulkCreateProjectResultDto();
        var allArticles = new List<CreateArticleDto>();

        // Phase 1: Create all projects
        foreach (var dto in items)
        {
            var entity = MapToEntity(dto);
            var id = await _projectRepository.AddAsync(entity);
            result.ProjectIds.Add(id);

            // Collect articles with assigned project ID
            if (dto.Articles != null && dto.Articles.Count > 0)
            {
                foreach (var article in dto.Articles)
                {
                    article.ProjectId = id;
                    allArticles.Add(article);
                }
            }
        }

        // Phase 2: Bulk insert all articles (ON CONFLICT skips duplicates by URL)
        if (allArticles.Count > 0)
        {
            var articleResult = await _articleRepository.BulkCreateAsync(allArticles);
            result.ArticlesInserted = articleResult.Inserted;
            result.ArticlesSkipped = articleResult.Skipped;
        }

        await _cacheService.RefreshProjectNamesCacheAsync();
        return result;
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter)
    {
        return await _projectRepository.GetProjectsAsync(filter);
    }

    public async Task<List<ProjectDto>> GetTopByConfirmationDateAsync(int top)
    {
        return await _projectRepository.GetTopByConfirmationDateAsync(top);
    }

    public async Task<bool> UpdateInvestorAsync(Guid id, string investor)
    {
        return await _projectRepository.UpdateInvestorAsync(id, investor);
    }

    public async Task<int> BulkUpdateInvestorAsync(List<BulkUpdateInvestorItemDto> items)
    {
        return await _projectRepository.BulkUpdateInvestorAsync(items);
    }

    public async Task<bool> UpdateConfirmationDateAsync(Guid id, DateTime date)
    {
        return await _projectRepository.UpdateConfirmationDateAsync(id, date);
    }

    public async Task<int> BulkUpdateConfirmationDateAsync(List<BulkUpdateConfirmationDateItemDto> items)
    {
        return await _projectRepository.BulkUpdateConfirmationDateAsync(items);
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id)
    {
        return await _projectRepository.GetProjectByIdAsync(id);
    }

    public Task<bool> UpdateAsync(Guid id, UpdateProjectDto dto)
        => _projectRepository.UpdateProjectAsync(id, dto);

    public async Task<bool> HideAsync(Guid id)
    {
        return await _projectRepository.HideAsync(id);
    }

    public async Task<bool> ShowAsync(Guid id)
    {
        return await _projectRepository.ShowAsync(id);
    }

    public Task<PagedResult<ArticleDto>> GetProjectArticlesAsync(Guid projectId, int page, int pageSize)
        => _projectRepository.GetProjectArticlesAsync(projectId, page, pageSize);

    public Task<PagedResult<TiktokMentionDto>> GetProjectTiktokVideosAsync(Guid projectId, int page, int pageSize)
        => _projectRepository.GetProjectTiktokVideosAsync(projectId, page, pageSize);

    public Task<PagedResult<YoutubeMentionDto>> GetProjectYoutubeVideosAsync(Guid projectId, int page, int pageSize)
        => _projectRepository.GetProjectYoutubeVideosAsync(projectId, page, pageSize);

    public Task<PagedResult<ProjectDto>> GetProjectsRankedByMentionsAsync(int page, int pageSize)
        => _projectRepository.GetProjectsRankedByMentionsAsync(page, pageSize);

    private static ProjectEntity MapToEntity(CreateProjectDto dto)
    {
        return new ProjectEntity
        {
            Name = dto.Name,
            Source = dto.Source,
            Location = dto.Location,
            Investor = dto.Investor,
            OriginUrl = dto.OriginUrl,
            Logo = dto.Logo,
            CategoryKey = dto.CategoryKey,
            ConfirmationDate = dto.ConfirmationDate,
            ProvinceId = dto.ProvinceId
        };
    }
}
