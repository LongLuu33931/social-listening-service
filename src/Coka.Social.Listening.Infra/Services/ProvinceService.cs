using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Interfaces.Repositories;
using Coka.Social.Listening.Core.Interfaces.Services;

namespace Coka.Social.Listening.Infra.Services;

public class ProvinceService : IProvinceService
{
    private readonly IProvinceRepository _provinceRepository;

    public ProvinceService(IProvinceRepository provinceRepository)
    {
        _provinceRepository = provinceRepository;
    }

    public Task<List<ProvinceDto>> GetAllAsync(ProvinceFilterDto filter)
        => _provinceRepository.GetAllAsync(filter);

    public Task<ProvinceDto?> GetByIdAsync(Guid id)
        => _provinceRepository.GetByIdAsync(id);

    public Task<ProvinceDto?> GetBySlugAsync(string slug)
        => _provinceRepository.GetBySlugAsync(slug);
}
