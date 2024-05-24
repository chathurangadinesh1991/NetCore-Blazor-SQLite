using AutoMapper;
using Chinook.ClientModels;
using Chinook.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class ArtistService : IArtistService
    {
        IDbContextFactory<ChinookContext> DbFactory { get; set; }
        IMapper Mapper { get; set; }

        public ArtistService(IDbContextFactory<ChinookContext> dbFactory, IMapper mapper)
        {
            DbFactory = dbFactory;
            Mapper = mapper;
        }

        public async Task<Artist> GetArtistById(long artistId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            var artist = dbContext.Artists.SingleOrDefault(a => a.ArtistId == artistId);
            return Mapper.Map<Artist>(artist);
        }

        public async Task<List<long>> GetArtistIdsByName(string searchTerm)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext.Artists.Where(a => a.Name.ToLower().Contains(searchTerm.Trim().ToLower())).Select(b => b.ArtistId).ToList();
        }

        public async Task<List<Artist>> GetArtists()
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            var artists = dbContext.Artists
                .Include(a => a.Albums)
            .ToList();
            return Mapper.Map<List<Artist>>(artists);
        }        
    }
}
