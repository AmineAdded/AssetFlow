// ============================================================
// AssetFlow.BlazorUI / Components / LoadingScreen.razor.cs
// Gère la visibilité de l'écran de chargement
// ============================================================

using Microsoft.AspNetCore.Components;

namespace AssetFlow.BlazorUI.Components
{
    public partial class LoadingScreen
    {
        // Contrôle si le loading est affiché
        private bool IsVisible { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            // Attendre 3 secondes (durée de l'animation CSS)
            await Task.Delay(3000);
            IsVisible = false;
            StateHasChanged(); // Forcer le re-rendu
        }
    }
}