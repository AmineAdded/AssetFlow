// ============================================================
// Pages/Employe/SignalerIncident.razor.cs
//
// MODIFICATIONS PAR RAPPORT À L'ORIGINAL :
//   1. Route optionnelle : AffectationId n'est plus obligatoire
//   2. OnInitializedAsync charge GetMesEquipementsAsync()
//   3. Si AffectationId est fourni dans l'URL → pré-sélection dans la dropdown
//   4. La validation vérifie que SelectedAffectationId > 0
//
// AUCUN changement dans le design ou la logique de soumission.
// ============================================================

using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;

namespace AssetFlow.BlazorUI.Pages.Employe
{
    public partial class SignalerIncident
    {
        // ── Paramètre URL optionnel ────────────────────────────
        // Renseigné depuis DetailsEquipement via NaviguerVersSignalement()
        // Vaut 0 si on arrive depuis /employe/incident (sidebar)
        [Parameter] public int AffectationId { get; set; } = 0;

        // ── Injections ─────────────────────────────────────────
        [Inject] private IncidentService   IncidentService { get; set; } = default!;
        [Inject] private EmployeService    EmployeService  { get; set; } = default!;
        [Inject] private NavigationManager Navigation      { get; set; } = default!;

        // ── Données dropdown ───────────────────────────────────
        // Liste de tous les équipements affectés à l'utilisateur connecté
        private List<EquipementAffecteDto> Equipements { get; set; } = new();

        // Valeur liée au <select> — initialisée à AffectationId si fourni
        private int SelectedAffectationId { get; set; } = 0;

        // ── Formulaire ──────────────────────────────────────────
        private string TypeIncident { get; set; } = "Panne";
        private string Description  { get; set; } = string.Empty;
        private int    Urgence      { get; set; } = 50;

        // ── États ───────────────────────────────────────────────
        private bool   IsLoading    { get; set; } = true;
        private bool   IsSubmitting { get; set; } = false;
        private string ErrorMessage { get; set; } = string.Empty;

        // ── Infos utilisateur ──────────────────────────────────
        private string UserName { get; set; } = "Utilisateur";

        // ── Initialisation ─────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            // 1. Nom de l'utilisateur pour l'avatar
            UserName = await EmployeService.GetCurrentUserNameAsync();

            // 2. Chargement des équipements depuis l'API
            try
            {
                Equipements = await EmployeService.GetMesEquipementsAsync();
            }
            catch
            {
                Equipements = new List<EquipementAffecteDto>();
            }

            // 3. Pré-sélection :
            //    - Si AffectationId > 0 (arrivé depuis DetailsEquipement),
            //      on sélectionne cet équipement dans la dropdown.
            //    - Sinon (arrivé depuis la sidebar), rien de sélectionné.
            if (AffectationId > 0 && Equipements.Any(e => e.AffectationId == AffectationId))
            {
                SelectedAffectationId = AffectationId;
            }

            IsLoading = false;
        }

        // ── Sélection du type d'incident ───────────────────────
        private void SelectType(string type)
        {
            TypeIncident = type;
            StateHasChanged();
        }

        // ── Slider urgence ─────────────────────────────────────
        private void OnUrgencyChange(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int value))
            {
                Urgence = value;
                StateHasChanged();
            }
        }

        // ── Soumission ─────────────────────────────────────────
        private async Task SoumettreIncident()
        {
            ErrorMessage = string.Empty;

            // Validation : un équipement doit être sélectionné
            if (SelectedAffectationId <= 0)
            {
                ErrorMessage = "Veuillez sélectionner un équipement.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Veuillez décrire le problème.";
                return;
            }

            try
            {
                IsSubmitting = true;

                var result = await IncidentService.SignalerIncidentAsync(new SignalerIncidentRequestDto
                {
                    AffectationId = SelectedAffectationId,
                    TypeIncident  = TypeIncident,
                    Urgence       = Urgence,
                    Description   = Description
                });

                if (result.Success)
                {
                    // Redirection vers la page de succès (inchangée)
                    Navigation.NavigateTo($"/employe/incident/success?numero={result.NumeroIncident}");
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        // ── Helpers UI ─────────────────────────────────────────
        private string GetUrgencyLabel()
        {
            if (Urgence <= 33) return "FAIBLE";
            if (Urgence <= 66) return "MOYEN";
            return "CRITIQUE";
        }

        private string GetUrgencyClass()
        {
            if (Urgence <= 33) return "urgency-low";
            if (Urgence <= 66) return "urgency-medium";
            return "urgency-critical";
        }

        private string GetUserInitials()
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2)
                return parts[0][..2].ToUpper();
            return "??";
        }
    }
}