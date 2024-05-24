using AutoMapper;

namespace Chinook.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Models.Artist, ClientModels.Artist>();
            CreateMap<ClientModels.Artist, Models.Artist>();

            CreateMap<Models.Playlist, ClientModels.Playlist>();
            CreateMap<ClientModels.Playlist, Models.Playlist>();

            CreateMap<Models.Album, ClientModels.Album>();
            CreateMap<ClientModels.Album, Models.Album>();
        }
    }
}
