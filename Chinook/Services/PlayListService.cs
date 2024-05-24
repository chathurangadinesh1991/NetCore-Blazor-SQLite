using AutoMapper;
using Chinook.ClientModels;
using Chinook.Helpers;
using Chinook.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class PlayListService : IPlayListService
    {
        IDbContextFactory<ChinookContext> DbFactory { get; set; }
        IMapper Mapper { get; set; }

        public PlayListService(IDbContextFactory<ChinookContext> dbFactory, IMapper mapper)
        {
            DbFactory = dbFactory;
            Mapper = mapper;
        }

        public async Task<List<Playlist>> GetPlaylistsByUserId(string UserId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext.Playlists
                    .Join(
                        dbContext.UserPlaylists,
                        pl => pl.PlaylistId,
                        upl => upl.Playlist.PlaylistId,
                        (pl, upl) => new
                        {
                            PlaylistName = pl.Name,
                            UserId = upl.UserId,
                            PlaylistId = pl.PlaylistId
                        }
                    )
                    .Where(a => a.PlaylistName != Constants.FAVORITE_PLAYLIST_NAME && a.UserId == UserId).
                    Select(b => new Playlist
                    {
                        PlaylistId = b.PlaylistId,
                        Name = b.PlaylistName
                    })
                    .ToList();
        }

        public async Task<List<Playlist>> GetPlaylists(string? userId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            //Get user playlists
            var playlist1 = dbContext.Playlists
                .Include(a => a.UserPlaylists)
                .Where(b => b.UserPlaylists.Any(c => c.UserId == userId)).ToList();

            //Include common playlists start
            //var playlist2 = dbContext.Playlists
            //    .Include(a => a.UserPlaylists)
            //    .Where(b => b.UserPlaylists.Count == 0).ToList();
            //playlist1.AddRange(playlist2);
            //Include common playlists end

            return Mapper.Map<List<Playlist>>(playlist1);
        }

        public async Task<Playlist> GetPlaylistById(long playlistId, string? userId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext.Playlists
               .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
               .Where(p => p.PlaylistId == playlistId)
               .Select(p => new Playlist()
               {
                   Name = p.Name,
                   Tracks = p.Tracks.Select(t => new PlaylistTrack()
                   {
                       AlbumTitle = t.Album.Title,
                       ArtistName = t.Album.Artist.Name,
                       TrackId = t.TrackId,
                       TrackName = t.Name,
                       IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == Constants.FAVORITE_PLAYLIST_NAME)).Any()
                   }).ToList()
               })
               .FirstOrDefault();
        }

        public async Task<bool> AddTrackToExistingPlaylist(int selectedPlaylistId, long trackId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            var selectedTrack = dbContext.Tracks.Where(a => a.TrackId == trackId).First();
            var selectedPlaylist = dbContext.Playlists.Where(a => a.PlaylistId == selectedPlaylistId).First();
            selectedPlaylist.Tracks.Add(selectedTrack);
            dbContext.SaveChanges();

            return true;
        }

        public async Task<bool> AddTrackToNewPlaylist(string playlistName, long trackId, string userId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            long maxPlaylistId = dbContext.Playlists.OrderByDescending(a => a.PlaylistId).FirstOrDefault()?.PlaylistId ?? 0;
            var selectedTrack = dbContext.Tracks.Where(a => a.TrackId == trackId).First();
            var playlist = new Models.Playlist
            {
                Name = playlistName,
                PlaylistId = maxPlaylistId + 1,
                Tracks = new List<Models.Track>() { selectedTrack }
            };

            dbContext.UserPlaylists.Add(new Models.UserPlaylist
            {
                UserId = userId,
                Playlist = playlist
            });
            dbContext.SaveChanges();

            return true;
        }

        public async Task<bool> IsTrackAvailableInPlaylistById(int selectedPlaylistId, long trackId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            var selectedTrack = dbContext.Tracks.Where(a => a.TrackId == trackId).First();
            return dbContext.Playlists.Any(a => a.PlaylistId == selectedPlaylistId && a.Tracks.Contains(selectedTrack));
        }

        public async Task<bool> IsUserPlaylistAvailableByName(string playlistName, string userId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext.UserPlaylists               
                .Include(a => a.Playlist)
                .Where(b => b.UserId == userId && b.Playlist.Name == playlistName)
                .Any();
        }

        public async Task<bool> RemoveTrackFromPlaylistById(long playlistId, long trackId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            var track = dbContext.Tracks.FirstOrDefault(a => a.TrackId == trackId);
            var userPlaylist = dbContext.Playlists
                        .Include(a => a.Tracks)
                                       .Where(b => b.PlaylistId.Equals(playlistId)).First();
            userPlaylist.Tracks.Remove(track);
            dbContext.SaveChanges();
            return true;
        }
    }
}
