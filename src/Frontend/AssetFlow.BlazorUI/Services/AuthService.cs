// ============================================================
// AssetFlow.BlazorUI / Services / AuthService.cs
// Service qui appelle l'API backend pour login/register
// ============================================================

using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace AssetFlow.BlazorUI.Services
{
    /// <summary>DTO pour envoyer la demande de login au backend</summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>DTO pour recevoir la réponse du backend après login</summary>
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>DTO pour l'inscription</summary>
    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string RequestedRole { get; set; } = string.Empty;
    }

    /// <summary>DTO pour la réponse d'inscription</summary>
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service d'authentification côté Blazor
    /// Communique avec l'API backend
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        /// <summary>
        /// Appelle POST api/auth/login et stocke le token localement
        /// </summary>
        public async Task<(bool Success, string Message)> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null)
                    {
                        // Stocker le token dans le localStorage du navigateur
                        await _localStorage.SetItemAsync("access_token", result.AccessToken);
                        await _localStorage.SetItemAsync("user_role", result.Role);
                        await _localStorage.SetItemAsync("user_name", result.FullName);
                        return (true, "Connexion réussie");
                    }
                }

                return (false, "Email ou mot de passe incorrect.");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur réseau: {ex.Message}");
            }
        }

        /// <summary>
        /// Appelle POST api/auth/register
        /// </summary>
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
                var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();

                if (result != null)
                    return (result.Success, result.Message);

                return (false, "Erreur inconnue.");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur réseau: {ex.Message}");
            }
        }

        /// <summary>Déconnexion : supprime le token local</summary>
        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("access_token");
            await _localStorage.RemoveItemAsync("user_role");
            await _localStorage.RemoveItemAsync("user_name");
        }

        /// <summary>Vérifie si l'utilisateur est connecté</summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("access_token");
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>Récupère le rôle stocké</summary>
        public async Task<string> GetUserRoleAsync()
        {
            return await _localStorage.GetItemAsync<string>("user_role") ?? string.Empty;
        }
    }
}