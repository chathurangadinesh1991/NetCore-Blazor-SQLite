using Chinook.ClientModels;

namespace Chinook.Interfaces
{
    public interface IArtistService
    {
        Task<Artist> GetArtistById(long artistId);
        Task<List<long>> GetArtistIdsByName(string searchTerm);
        Task<List<Artist>> GetArtists();
    }
}
