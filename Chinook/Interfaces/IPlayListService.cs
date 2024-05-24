using Chinook.ClientModels;

namespace Chinook.Interfaces
{
    public interface IPlayListService
    {
        Task<List<Playlist>> GetPlaylistsByUserId(string userId);

        Task<Playlist> GetPlaylistById(long playlistId,string? userId);

        Task<List<Playlist>> GetPlaylists(string? userId);

        Task<bool> AddTrackToExistingPlaylist(int selectedPlaylistId,long trackId);

        Task<bool> AddTrackToNewPlaylist(string playlistName, long trackId, string userId);

        Task<bool> IsTrackAvailableInPlaylistById(int selectedPlaylistId, long trackId);

        Task<bool> IsUserPlaylistAvailableByName(string playlistName, string userId);

        Task<bool> RemoveTrackFromPlaylistById(long playlistId, long trackId);
    }
}
