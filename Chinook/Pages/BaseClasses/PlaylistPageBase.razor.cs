using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Chinook.Interfaces;
using Chinook.ClientModels;
using Microsoft.IdentityModel.Tokens;
using Chinook.Helpers;

namespace Chinook.Pages.BaseClasses
{
    public class PlaylistPageBase : ComponentBase
    {
        [Parameter] public long PlaylistId { get; set; }       
        [CascadingParameter] private Task<AuthenticationState> AuthenticationState { get; set; }
        [CascadingParameter] public ApplicationStateChange StateChange { get; set; }
        [Inject] IPlayListService PlayListService { get; set; }
        [Inject] ITrackService TrackService { get; set; }
        [Inject] ILogger<PlaylistPageBase> Logger { get; set; }

        protected Playlist Playlist;
        protected string InfoMessage;

        private string CurrentUserId;     

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            CurrentUserId = await GetUserId();
        }

        protected override async Task OnParametersSetAsync()
        {
            await SetPlaylist();
            CloseInfoMessage();
        }

        private async Task<string> GetUserId()
        {
            var user = (await AuthenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private async Task SetPlaylist()
        {
            try
            {
                if (PlaylistId < 1)
                    throw new ArgumentException(Messages.InvalidPlaylist);
                if (CurrentUserId.IsNullOrEmpty())
                    throw new ArgumentException(Messages.InvalidUser);

                Playlist = await PlayListService.GetPlaylistById(PlaylistId, CurrentUserId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SetPlaylist");
                InfoMessage = Messages.FailedObtainInformation;
                Playlist = null;
            }
        }

        protected async void FavoriteTrack(long trackId)
        {
            try
            {
                if (trackId < 1)
                    throw new ArgumentException(Messages.InvalidTrack);
                if(CurrentUserId.IsNullOrEmpty())
                    throw new ArgumentException(Messages.InvalidUser);
                if (PlaylistId < 1)
                    throw new ArgumentException(Messages.InvalidPlaylist);

                Playlist = await PlayListService.GetPlaylistById(PlaylistId, CurrentUserId);

                var track = Playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);

                bool isSuccessfull = await TrackService.UpdateFavoriteTrack(trackId, CurrentUserId);

                //If the database update process is successful and reaches this point
                if (isSuccessfull)
                {
                    track.IsFavorite = true;
                    InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";
                }
                else
                {
                    InfoMessage = Messages.FavouritesNotUpdated;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "FavoriteTrack");
                InfoMessage = Messages.FavouritesNotUpdated;
            }
            finally
            {
                DispachValuesChanged();
            }
        }

        protected async void UnfavoriteTrack(long trackId)
        {
            try
            {
                if (trackId < 1)
                    throw new ArgumentException(Messages.InvalidTrack);
                if (CurrentUserId.IsNullOrEmpty())
                    throw new ArgumentException(Messages.InvalidUser);

                var track = Playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);
                bool isSuccessfull = await TrackService.UpdateUnFavoriteTrack(trackId, CurrentUserId);

                //If the database update process is successful and reaches this point
                if (isSuccessfull)
                {
                    track.IsFavorite = false;
                    InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";

                    if(Playlist.Name == Constants.FAVORITE_PLAYLIST_NAME)
                        SetPlaylist();
                }
                else
                {
                    InfoMessage = Messages.FavouritesNotUpdated;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "UnfavoriteTrack");
                InfoMessage = Messages.FavouritesNotUpdated;
            }           
            finally
            {
                DispachValuesChanged();
            }
        }

        protected async void RemoveTrack(long trackId)
        {
            try
            {
                if (trackId < 1)
                    throw new ArgumentException(Messages.InvalidTrack);
                if (PlaylistId < 1)
                    throw new ArgumentException(Messages.InvalidPlaylist);

                bool isSuccessfull = await PlayListService.RemoveTrackFromPlaylistById(PlaylistId,trackId);

                if (isSuccessfull)
                {
                    InfoMessage = $"Successfully removed.";
               
                    await SetPlaylist();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "RemoveTrack");
                InfoMessage = $"Error occurring during track removal.";
            }
        }

        protected void CloseInfoMessage()
        {
            InfoMessage = "";
        }

        private void DispachValuesChanged()
        {
            StateChange.ValuesChanged.Invoke();
        }
    }
}
