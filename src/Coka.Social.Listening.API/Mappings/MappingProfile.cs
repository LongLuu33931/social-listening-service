using AutoMapper;
using Coka.Social.Listening.Core.DTOs;
using Coka.Social.Listening.Core.Entities;

namespace Coka.Social.Listening.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserEntity, UserDto>();
    }
}
