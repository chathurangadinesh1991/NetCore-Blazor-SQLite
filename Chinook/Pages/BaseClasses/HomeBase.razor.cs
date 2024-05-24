using Microsoft.AspNetCore.Components;
using Chinook.Interfaces;
using Chinook.ClientModels;
using Chinook.Helpers;

namespace Chinook.Pages.BaseClasses
{
    public class HomeBase : ComponentBase
    {        
        [Inject] IArtistService ArtistService { get; set; }
        [Inject] ILogger<HomeBase> Logger { get; set; }

        protected List<Artist> Artists;
        protected string InfoMessage;

        private List<Artist> BackUpArtists;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            Artists = await GetArtists();
            BackUpArtists = Artists;
        }

        private async Task<List<Artist>> GetArtists()
        {
            try
            {
                return await ArtistService.GetArtists();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetArtists");
                InfoMessage = Messages.FailedObtainInformation;
                return new List<Artist>();
            }
        }

        protected void SearchArtistsChanged(List<long> artistsIds)
        {
            try
            {
                if(artistsIds.Count > 0 && BackUpArtists.Count > 0 ) { }
                    Artists = BackUpArtists.Where(a => artistsIds.Any(b => b == a.ArtistId)).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SearchArtistsChanged");
                InfoMessage = $"There is an issue with search option.";
                Artists = BackUpArtists;
            }
        }

        protected void CloseInfoMessage()
        {
            InfoMessage = "";
        }
    }
}
