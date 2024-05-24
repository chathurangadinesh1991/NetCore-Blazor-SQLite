using AutoMapper;
using Chinook.ClientModels;
using Chinook.Helpers;
using Chinook.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class TrackService : ITrackService
    {
        IDbContextFactory<ChinookContext> DbFactory { get; set; }
        IMapper Mapper { get; set; }

        public TrackService(IDbContextFactory<ChinookContext> dbFactory, IMapper mapper)
        {
            DbFactory = dbFactory;
            Mapper = mapper;
        }

        public async Task<List<PlaylistTrack>> GetTracksByArtistId(long artistId, string? userId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext.Tracks.Where(a => a.Album.ArtistId == artistId)
                .Include(a => a.Album)
                .Select(t => new PlaylistTrack()
                {
                    AlbumTitle = t.Album == null ? "-" : t.Album.Title,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => 
                        up.UserId == userId && 
                        up.Playlist.Name == Constants.FAVORITE_PLAYLIST_NAME)
                    ).Any()
                })
                .ToList();
        }

        public async Task<bool> UpdateFavoriteTrack(long trackId, string userId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
                        
            var track = dbContext.Tracks.FirstOrDefault(a => a.TrackId == trackId);
            var userFavPlaylist = dbContext.Playlists
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
                .Where(a => a.PlaylistName == Constants.FAVORITE_PLAYLIST_NAME && a.UserId == userId)
                .FirstOrDefault();

            if (userFavPlaylist == null)
            {
                //The user has not made a favourite playlist.
                //If the primary key's auto incrementation is enabled, there is no need to take the next ID.
                long maxPlaylistId = dbContext.Playlists.OrderByDescending(a => a.PlaylistId).FirstOrDefault()?.PlaylistId ?? 0;

                var playlist = new Models.Playlist
                {
                    Name = Constants.FAVORITE_PLAYLIST_NAME,
                    PlaylistId = maxPlaylistId + 1,
                    Tracks = new List<Models.Track>() { track }
                };

                dbContext.UserPlaylists.Add(new Models.UserPlaylist
                {
                    UserId = userId,
                    Playlist = playlist
                });
                dbContext.SaveChanges();
            }
            else
            {
                var userFavPlaylistObject = dbContext.Playlists
                    .Where(a => a.PlaylistId.Equals(userFavPlaylist.PlaylistId)).First();
                userFavPlaylistObject.Tracks.Add(track);
                dbContext.SaveChanges();
            }

            return true;
        }

        public async Task<bool> UpdateUnFavoriteTrack(long trackId, string userId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
           
            var track = dbContext.Tracks.FirstOrDefault(a => a.TrackId == trackId);
            var userFavPlaylist = dbContext.Playlists
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
                .Where(a => a.PlaylistName == Constants.FAVORITE_PLAYLIST_NAME && a.UserId == userId)
                .FirstOrDefault();

            if (userFavPlaylist != null)
            {
                var _userFavPlaylistObject = dbContext.Playlists
                        .Include(a => a.Tracks)
                        .Where(b => b.PlaylistId.Equals(userFavPlaylist.PlaylistId)).First();
                _userFavPlaylistObject.Tracks.Remove(track);
                dbContext.SaveChanges();
            }

            return true;
        }
    }
}