using Chinook.ClientModels;
using Chinook.Helpers;
using Chinook.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Chinook.Shared.BaseClasses
{
    public class NavMenuBase : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState> AuthenticationState { get; set; }
        [CascadingParameter] public ApplicationStateChange StateChange { get; set; } 
        [Inject] IPlayListService PlayListService { get; set; }
        [Inject] ILogger<NavMenuBase> Logger { get; set; }

        protected bool collapseNavMenu = true;  
        protected string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;
        protected List<Playlist> UserPlaylist;
        protected string InfoMessage;

        private string CurrentUserId = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            CurrentUserId = await GetUserId();
            StateChange.ValuesChanged += () => SetUserPlaylist();
            SetUserPlaylist();
        }
        
        private async Task<string> GetUserId()
        {
            var user = (await AuthenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private async Task SetUserPlaylist()
        {
            try
            {
                if (CurrentUserId.IsNullOrEmpty())
                    throw new ArgumentException(Messages.InvalidUser);

                UserPlaylist = await PlayListService.GetPlaylists(CurrentUserId);
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SetUserPlaylist");
                InfoMessage = Messages.FailedObtainInformation;
            }
        }

        protected void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }
    }
}
