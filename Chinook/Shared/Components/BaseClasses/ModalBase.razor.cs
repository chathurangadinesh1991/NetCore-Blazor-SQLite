using Microsoft.AspNetCore.Components;

namespace Chinook.Shared.Components.BaseClasses
{
    public class ModalBase : ComponentBase
    {
        [Parameter] public RenderFragment? Title { get; set; }
        [Parameter] public RenderFragment? Body { get; set; }
        [Parameter] public RenderFragment? Footer { get; set; }

        public Guid Guid = Guid.NewGuid();

        protected string modalDisplay = "none;";
        protected string modalClass = "";
        protected bool showBackdrop = false;

        public void Open()
        {
            modalDisplay = "block;";
            modalClass = "show";
            showBackdrop = true;
            StateHasChanged();
        }

        public void Close()
        {
            modalDisplay = "none";
            modalClass = "";
            showBackdrop = false;
        }
    }
}
