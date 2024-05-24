using Microsoft.AspNetCore.Components;

namespace Chinook.Shared.Components.BaseClasses
{
    public class LoadingBase : ComponentBase
    {
        [Parameter] public string Message { get; set; }
    }
}
