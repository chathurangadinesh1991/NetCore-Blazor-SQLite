using Chinook.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Chinook.Shared.Components.BaseClasses
{
    public class SearchArtistBase : ComponentBase
    {
        [Parameter] public EventCallback<List<long>> SearchChanged { get; set; }
        [Inject] IArtistService ArtistService { get; set; }
        [Inject] ILogger<SearchArtistBase> Logger { get; set; }

        protected string SearchTerm;
        protected bool IsDisabled;

        public async Task SearchArtists()
        {
            try
            {
                IsDisabled = true;
                var artistsIds = await ArtistService.GetArtistIdsByName(SearchTerm ?? string.Empty);
                await SearchChanged.InvokeAsync(artistsIds);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SearchArtists");              
            }            
            finally
            {
                IsDisabled = false;
            }           
        }
    }
}
