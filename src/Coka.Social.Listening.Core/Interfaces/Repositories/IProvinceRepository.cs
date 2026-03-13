using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Repositories;

namespace Coka.Social.Listening.Core.Interfaces.Repositories;

public interface IProvinceRepository
{
    Task<List<ProvinceDto>> GetAllAsync(ProvinceFilterDto filter);
    Task<ProvinceDto?> GetByIdAsync(Guid id);
    Task<ProvinceDto?> GetBySlugAsync(string slug);
}
