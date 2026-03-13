using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;

namespace Coka.Social.Listening.Core.Interfaces.Repositories;

public interface ICategoryRepository : IBaseRepository<CategoryEntity>
{
    Task<List<CategoryGroupDto>> GetCategoryTreeAsync();
}
