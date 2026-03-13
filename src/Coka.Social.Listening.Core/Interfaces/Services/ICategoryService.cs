using Coka.Social.Listening.Core.DTOs;

namespace Coka.Social.Listening.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<List<CategoryGroupDto>> GetCategoryTreeAsync();
}
