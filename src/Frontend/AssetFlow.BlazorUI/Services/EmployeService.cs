// ============================================================
// AssetFlow.BlazorUI / Services / EmployeService.cs
// Service frontend pour les opérations employé
// Récupère user_id depuis localStorage
// ============================================================

using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace AssetFlow.BlazorUI.Services
{
    /// <summary>
    /// DTO pour recevoir les équipements depuis l'API
    /// </summary>
    public class EquipementAffecteDto
    {
        public int AffectationId { get; set; }
        public int MaterielId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime DateAffectation { get; set; }
        public int QuantiteAffectee { get; set; }
        public string Statut { get; set; } = string.Empty;
        public string StatutBadgeColor { get; set; } = string.Empty;
        public string? Observations { get; set; }
    }

    /// <summary>
    /// DTO pour signaler un incident
    /// </summary>
    public class SignalerIncidentDto
    {
        public int AffectationId { get; set; }
        public string TypeIncident { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateIncident { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Service employé côté frontend
    /// </summary>
    public class EmployeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public EmployeService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        /// <summary>
        /// Récupère l'ID de l'utilisateur connecté depuis le localStorage
        /// </summary>
        public async Task<int?> GetCurrentUserIdAsync()
        {
            return await _localStorage.GetItemAsync<int?>("user_id");
        }

        /// <summary>
        /// Récupère le nom complet de l'utilisateur
        /// </summary>
        public async Task<string> GetCurrentUserNameAsync()
        {
            return await _localStorage.GetItemAsync<string>("user_name") ?? "Utilisateur";
        }

        /// <summary>
        /// Récupère le rôle de l'utilisateur (traduit en français)
        /// </summary>
        public async Task<string> GetCurrentUserRoleAsync()
        {
            var role = await _localStorage.GetItemAsync<string>("user_role");
            return role switch
            {
                "Employe" => "Employé",
                "IT" => "Équipe IT",
                "EquipeAchat" => "Équipe Achat",
                "Admin" => "Administrateur",
                _ => role ?? "Employé"
            };
        }

        /// <summary>
        /// Récupère tous les équipements affectés à l'utilisateur connecté
        /// </summary>
        public async Task<List<EquipementAffecteDto>> GetMesEquipementsAsync()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    throw new Exception("Utilisateur non connecté");
                }

                var response = await _httpClient.GetAsync($"api/employe/equipements/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<EquipementAffecteDto>>();
                    return data ?? new List<EquipementAffecteDto>();
                }

                return new List<EquipementAffecteDto>();
            }
            catch
            {
                return new List<EquipementAffecteDto>();
            }
        }

        /// <summary>
        /// Récupère le détail d'une affectation
        /// </summary>
        public async Task<EquipementAffecteDto?> GetEquipementDetailAsync(int affectationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/employe/equipements/detail/{affectationId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EquipementAffecteDto>();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}