using Chinook.ClientModels;

namespace Chinook.Interfaces
{
    public interface ITrackService
    {
        Task<List<PlaylistTrack>> GetTracksByArtistId(long artistId,string? userId);

        Task<bool> UpdateFavoriteTrack(long trackId, string userId);

        Task<bool> UpdateUnFavoriteTrack(long trackId, string userId);
    }
}
