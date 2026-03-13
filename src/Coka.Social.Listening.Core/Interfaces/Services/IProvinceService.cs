using Coka.Social.Listening.Core.DTOs;

namespace Coka.Social.Listening.Core.Interfaces.Services;

public interface IProvinceService
{
    Task<List<ProvinceDto>> GetAllAsync(ProvinceFilterDto filter);
    Task<ProvinceDto?> GetByIdAsync(Guid id);
    Task<ProvinceDto?> GetBySlugAsync(string slug);
}
