// ============================================================
// AssetFlow.BlazorUI / Pages / Employe / MesEquipements.razor.cs
// MISE À JOUR : Utilise EmployeService + récup infos depuis localStorage
// ============================================================

using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;

namespace AssetFlow.BlazorUI.Pages.Employe
{
    public partial class MesEquipements
    {
        [Inject] private EmployeService EmployeService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        // === DONNÉES ===
        private List<EquipementAffecteDto> Equipements { get; set; } = new();
        private List<EquipementAffecteDto> EquipementsFiltres { get; set; } = new();
        
        private bool IsLoading { get; set; } = true;
        private string ErrorMessage { get; set; } = string.Empty;

        // === RECHERCHE ===
        private string SearchQuery { get; set; } = string.Empty;

        // === INFO UTILISATEUR (récupérées depuis localStorage) ===
        private string UserName { get; set; } = "Utilisateur";
        private string UserRole { get; set; } = "Employé";

        /// <summary>
        /// Chargement initial
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await LoadUserInfoAsync();
            await LoadEquipementsAsync();
        }

        /// <summary>
        /// Charge les infos utilisateur depuis localStorage
        /// </summary>
        private async Task LoadUserInfoAsync()
        {
            UserName = await EmployeService.GetCurrentUserNameAsync();
            UserRole = await EmployeService.GetCurrentUserRoleAsync();
        }

        /// <summary>
        /// Charge les équipements depuis l'API
        /// </summary>
        private async Task LoadEquipementsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                Equipements = await EmployeService.GetMesEquipementsAsync();
                EquipementsFiltres = Equipements; // Initialise la liste filtrée
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Gère la recherche en temps réel
        /// </summary>
        private void OnSearchInput(ChangeEventArgs e)
        {
            SearchQuery = e.Value?.ToString() ?? string.Empty;
            FiltrerEquipements();
        }

        /// <summary>
        /// Filtre les équipements selon la recherche
        /// </summary>
        private void FiltrerEquipements()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                EquipementsFiltres = Equipements;
            }
            else
            {
                var query = SearchQuery.ToLower();
                EquipementsFiltres = Equipements.Where(e =>
                    e.Designation.ToLower().Contains(query) ||
                    e.Reference.ToLower().Contains(query) ||
                    e.Categorie.ToLower().Contains(query)
                ).ToList();
            }
        }

        /// <summary>
        /// Navigue vers la page de détail
        /// </summary>
        private void VoirDetail(int affectationId)
        {
            Navigation.NavigateTo($"/employe/equipement/{affectationId}");
        }

        /// <summary>
        /// Traduit le statut en français
        /// </summary>
        private string GetStatutLabel(string statut)
        {
            return statut switch
            {
                "EnCours" => "BON",
                "Retourne" => "RETOURNÉ",
                "Perdu" => "PERDU",
                "Endommage" => "ENDOMMAGÉ",
                _ => statut.ToUpper()
            };
        }

        /// <summary>
        /// Génère les initiales pour l'avatar
        /// </summary>
        private string GetInitials()
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2)
                return parts[0].Substring(0, 2).ToUpper();
            return "??";
        }
    }
}