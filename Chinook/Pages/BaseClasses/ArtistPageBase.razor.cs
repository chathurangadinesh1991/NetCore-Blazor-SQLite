using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Chinook.Interfaces;
using Chinook.ClientModels;
using Chinook.Shared.Components.BaseClasses;
using Chinook.Helpers;

namespace Chinook.Pages.BaseClasses
{
    public class ArtistPageBase : ComponentBase
    {
        [Parameter] public long ArtistId { get; set; }
        [CascadingParameter] private Task<AuthenticationState> AuthenticationState { get; set; }
        [CascadingParameter] public ApplicationStateChange StateChange { get; set; }
        [Inject] IArtistService ArtistService { get; set; }
        [Inject] IPlayListService PlayListService { get; set; }
        [Inject] ITrackService TrackService { get; set; }
        [Inject] ILogger<ArtistPageBase> Logger { get; set; }

        protected ModalBase PlaylistDialog { get; set; }
        protected Artist Artist;
        protected List<PlaylistTrack> Tracks;
        protected List<Playlist> UserPlaylists;
        protected PlaylistTrack SelectedTrack;
        protected string InfoMessage;

        protected string NewPlaylistName;
        protected bool IsDisabled;
        protected int SelectedPlaylist = -1;

        private string CurrentUserId;        

        protected override async Task OnInitializedAsync()
        {            
            await InvokeAsync(StateHasChanged);
            CurrentUserId = await GetUserId();
            Artist = await GetArtist();
            UserPlaylists = await GetUserPlaylists();
            Tracks = await GetTracks();
        }

        private async Task<string> GetUserId()
        {
            var user = (await AuthenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private async Task<Artist> GetArtist()
        {
            try
            {
                if (ArtistId < 1)
                    throw new ArgumentException(Messages.InvalidArtist);    

                return await ArtistService.GetArtistById(ArtistId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetArtist");
                InfoMessage = Messages.FailedObtainInformation;
                return new Artist { ArtistId = 0, Name = "Undefined", Albums = new List<Album>() };
            }
        }

        private async Task<List<Playlist>> GetUserPlaylists()
        {            
            try
            {
                if (CurrentUserId.IsNullOrEmpty())
                    throw new ArgumentException(Messages.InvalidUser);

                return await PlayListService.GetPlaylistsByUserId(CurrentUserId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetUserPlaylists");
                InfoMessage = Messages.FailedObtainInformation;
                return new List<Playlist>();
            }
        }

        private async Task<List<PlaylistTrack>> GetTracks()
        {
            try
            {
                if (ArtistId < 1)
                    throw new ArgumentException(Messages.InvalidArtist);              

                return await TrackService.GetTracksByArtistId(ArtistId,CurrentUserId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetTracks");
                InfoMessage = Messages.FailedObtainInformation;
                return new List<PlaylistTrack>();
            }
        }

        protected async void FavoriteTrack(long trackId)
        {
            try
            {
                if (trackId < 1)
                    throw new ArgumentException(Messages.InvalidTrack);
                if (CurrentUserId.IsNullOrEmpty())
                    throw new ArgumentException(Messages.InvalidUser);

                var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);
                bool isSuccessfull =  await TrackService.UpdateFavoriteTrack(trackId, CurrentUserId);

                //If the database update process is successful and reaches this point
                if(isSuccessfull)
                {
                    track.IsFavorite = true;
                    InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";
                }
                else
                {
                    InfoMessage = Messages.FavouritesNotUpdated;
                }               
            }
            catch(Exception ex)
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

                var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);
                bool isSuccessfull = await TrackService.UpdateUnFavoriteTrack(trackId, CurrentUserId);

                //If the database update process is successful and reaches this point
                if (isSuccessfull)
                {
                    track.IsFavorite = false;
                    InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";
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
     
        protected async void AddTrackToPlaylist()
        {
            try
            {
                if (CurrentUserId.IsNullOrEmpty())
                    throw new ArgumentException(Messages.InvalidUser);
                if (SelectedTrack.TrackId < 1)
                    throw new ArgumentException("Selected track is invalid");

                string infoMessage = string.Empty;

                if (SelectedPlaylist == null || SelectedPlaylist == -1)
                {
                    //Add track into new playlist
                    if (NewPlaylistName.IsNullOrEmpty())
                    {
                        infoMessage = $"To proceed, please choose one of the existing playlists or enter the name of a new playlist.";
                    }                        
                    else
                    {
                        bool isPlaylistAvailable = await PlayListService.IsUserPlaylistAvailableByName(NewPlaylistName, CurrentUserId);
                        if (isPlaylistAvailable)
                        {
                            infoMessage = $"Playlist is alrady avalable. Please use another name";
                        }
                        else
                        {
                            bool isSuccessfull = await PlayListService.AddTrackToNewPlaylist(NewPlaylistName, SelectedTrack.TrackId, CurrentUserId);
                            if (isSuccessfull)
                            {
                                infoMessage = $"Track {Artist.Name} - {SelectedTrack.AlbumTitle} - {SelectedTrack.TrackName} added to playlist {NewPlaylistName}.";
                                PlaylistDialog.Close();
                                UserPlaylists = await GetUserPlaylists();
                            }
                        }
                    }
                }
                else
                {
                    //Add track to existing playlist
                    bool isTrackAvailable = await PlayListService.IsTrackAvailableInPlaylistById(SelectedPlaylist, SelectedTrack.TrackId);

                    if (isTrackAvailable)
                    {
                        infoMessage = $"Track {Artist.Name} - {SelectedTrack.AlbumTitle} - {SelectedTrack.TrackName} is alrady avalable in the playlist.";
                    }
                    else
                    {
                        bool isSuccessfull = await PlayListService.AddTrackToExistingPlaylist(SelectedPlaylist, SelectedTrack.TrackId);
                        if (isSuccessfull)
                        {
                            infoMessage = $"Track {Artist.Name} - {SelectedTrack.AlbumTitle} - {SelectedTrack.TrackName} added to the selected playlist.";
                            PlaylistDialog.Close();
                        }                            
                    }
                }

                //If the database update process is successful and reaches this point
                CloseInfoMessage();
                InfoMessage = infoMessage;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "AddTrackToPlaylist");
                InfoMessage = $"The Playlist was not updated.";              
            }
            finally
            {
                DispachValuesChanged();
            }
        }

        protected void OpenPlaylistDialog(long trackId)
        {
            CloseInfoMessage();
            NewPlaylistName = string.Empty;
            SelectedPlaylist = -1;

            if (trackId > 0)
            {
                SelectedTrack = Tracks.FirstOrDefault(t => t.TrackId == trackId);
                PlaylistDialog.Open();
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
