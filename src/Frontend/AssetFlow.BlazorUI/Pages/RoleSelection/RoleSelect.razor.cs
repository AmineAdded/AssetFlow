// ============================================================
// AssetFlow.BlazorUI / Pages / Auth / RoleSelect.razor.cs
// Logique de la page de sélection de rôle
// Séparation claire : UI dans .razor, logique ici
// ============================================================

using Microsoft.AspNetCore.Components;

namespace AssetFlow.BlazorUI.Pages.Auth
{
    public partial class RoleSelect
    {
        // Injection de NavigationManager pour naviguer entre pages
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        // Valeur tapée par l'utilisateur (pour mode admin caché)
        private string AdminInput { get; set; } = string.Empty;

        /// <summary>
        /// Appelée quand l'utilisateur clique sur "Continuer" d'un rôle
        /// Redirige vers la page de login avec le rôle choisi
        /// </summary>
        private void SelectRole(string role)
        {
            // Naviguer vers la page Login en passant le rôle dans l'URL
            Navigation.NavigateTo($"/login?role={role}");
        }

        /// <summary>
        /// Vérifie si l'utilisateur tape "admin" pour accéder au mode admin
        /// </summary>
        private void CheckAdminMode(ChangeEventArgs e)
        {
            AdminInput = e.Value?.ToString() ?? string.Empty;

            if (AdminInput.ToLower() == "admin")
            {
                Navigation.NavigateTo("/login?role=Admin");
            }
        }
    }
}