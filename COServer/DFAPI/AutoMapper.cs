using API.Models.GameServer;
using AutoMapper;
using Core.Models.GameServer;

namespace API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApiPlayerItem, PlayerItem>().ReverseMap();
        }
    }
}
