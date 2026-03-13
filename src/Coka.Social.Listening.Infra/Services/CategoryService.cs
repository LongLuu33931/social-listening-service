using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Core.Interfaces.Services;

namespace Coka.Social.Listening.Infra.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryGroupDto>> GetCategoryTreeAsync()
    {
        return await _categoryRepository.GetCategoryTreeAsync();
    }
}
