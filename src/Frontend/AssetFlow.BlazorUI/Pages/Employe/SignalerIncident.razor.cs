// ============================================================
// AssetFlow.BlazorUI / Pages / Employe / SignalerIncident.razor.cs
// Code-behind pour le formulaire de signalement d'incident
// ============================================================

using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;

namespace AssetFlow.BlazorUI.Pages.Employe
{
    public partial class SignalerIncident
    {
        [Parameter] public int AffectationId { get; set; }

        [Inject] private IncidentService IncidentService { get; set; } = default!;
        [Inject] private EmployeService EmployeService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        // === DONNÉES ÉQUIPEMENT ===
        private string EquipementDesignation { get; set; } = string.Empty;
        private string EquipementReference { get; set; } = string.Empty;

        // === FORMULAIRE ===
        private string TypeIncident { get; set; } = "Panne";
        private string Description { get; set; } = string.Empty;
        private int Urgence { get; set; } = 50;

        // === ÉTATS ===
        private bool IsLoading { get; set; } = true;
        private bool IsSubmitting { get; set; } = false;
        private string ErrorMessage { get; set; } = string.Empty;

        // === USER INFO ===
        private string UserName { get; set; } = "Utilisateur";

        protected override async Task OnInitializedAsync()
        {
            await LoadUserInfo();
            await LoadEquipementInfo();
        }

        private async Task LoadUserInfo()
        {
            UserName = await EmployeService.GetCurrentUserNameAsync();
        }

        private async Task LoadEquipementInfo()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var equipement = await EmployeService.GetEquipementDetailAsync(AffectationId);

                if (equipement != null)
                {
                    EquipementDesignation = equipement.Designation;
                    EquipementReference = equipement.Reference;
                }
                else
                {
                    ErrorMessage = "Équipement introuvable.";
                }
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

        private void SelectType(string type)
        {
            TypeIncident = type;
            StateHasChanged();
        }

        private void OnUrgencyChange(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int value))
            {
                Urgence = value;
                StateHasChanged();
            }
        }

        private async Task SoumettreIncident()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Veuillez décrire le problème.";
                return;
            }

            try
            {
                IsSubmitting = true;
                ErrorMessage = string.Empty;

                var result = await IncidentService.SignalerIncidentAsync(new SignalerIncidentRequestDto
                {
                    AffectationId = AffectationId,
                    TypeIncident = TypeIncident,
                    Urgence = Urgence,
                    Description = Description
                });

                if (result.Success)
                {
                    // Rediriger vers la page de succès
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
                return parts[0].Substring(0, 2).ToUpper();
            return "??";
        }
    }
}