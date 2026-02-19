// ============================================================
// AssetFlow.Infrastructure / Services / KeycloakAuthService.cs
// Implémentation concrète du service d'auth avec Keycloak
// Keycloak tourne en LOCAL (hors-ligne) sur http://localhost:8080
// ============================================================

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using AssetFlow.Domain.Entities;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssetFlow.Infrastructure.Services
{
    public class KeycloakAuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;

        // On récupère l'URL Keycloak depuis appsettings.json
        private string KeycloakUrl => _config["Keycloak:Authority"]!; // ex: http://localhost:8080/realms/assetflow
        private string ClientId => _config["Keycloak:ClientId"]!;
        private string ClientSecret => _config["Keycloak:ClientSecret"]!;

        public KeycloakAuthService(HttpClient httpClient, AppDbContext dbContext, IConfiguration config)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _config = config;
        }

        /// <summary>
        /// Login : appelle Keycloak avec email+password, récupère le token JWT
        /// </summary>
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            // URL du token Keycloak (endpoint standard OpenID Connect)
            var tokenUrl = $"{KeycloakUrl}/protocol/openid-connect/token";

            // Données envoyées à Keycloak (format form-urlencoded)
            var formData = new Dictionary<string, string>
            {
                { "grant_type", "password" },        // Mode: direct access grant
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "username", request.Email },
                { "password", request.Password },
                { "scope", "openid profile email roles" }
            };

            // Appel HTTP vers Keycloak
            var response = await _httpClient.PostAsync(
                tokenUrl,
                new FormUrlEncodedContent(formData)
            );
            Console.WriteLine($"Keycloak response status: {response.StatusCode}");

            // Si Keycloak refuse (mauvais mdp, utilisateur bloqué...)
            if (!response.IsSuccessStatusCode)
                return null;

            // Désérialiser la réponse Keycloak
            var json = await response.Content.ReadAsStringAsync();
            var keycloakResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (keycloakResponse == null) return null;

            // Récupérer l'utilisateur depuis notre BD SQL Server
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            return new LoginResponseDto
            {
                AccessToken = keycloakResponse.access_token,
                RefreshToken = keycloakResponse.refresh_token,
                ExpiresIn = keycloakResponse.expires_in,
                Role = request.Role,
                FullName = user != null ? $"{user.FirstName} {user.LastName}" : request.Email
            };
        }

        /// <summary>
        /// Register : crée l'utilisateur dans Keycloak + SQL Server
        /// </summary>
        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // Vérifier si l'email existe déjà dans notre BD
            var exists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
            if (exists)
                return new RegisterResponseDto { Success = false, Message = "Cet email existe déjà." };

            // --- Étape 1 : Créer l'utilisateur dans Keycloak ---
            // Pour créer un user Keycloak, on a besoin d'un token Admin
            var adminToken = await GetAdminTokenAsync();
            if (adminToken == null)
                return new RegisterResponseDto { Success = false, Message = "Erreur connexion Keycloak admin." };

            // URL admin Keycloak pour créer un user
            var createUserUrl = $"http://localhost:8080/admin/realms/assetflow/users";

            // Corps de la requête de création
            var keycloakUser = new
            {
                username = request.Email,
                email = request.Email,
                firstName = request.FirstName,
                lastName = request.LastName,
                enabled = true, // Activé de suite (ou false si approbation admin requise)
                credentials = new[]
                {
                    new { type = "password", value = request.Password, temporary = false }
                }
            };

            // Appel API Keycloak avec le token admin
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createResponse = await _httpClient.PostAsync(
                createUserUrl,
                new StringContent(JsonSerializer.Serialize(keycloakUser), Encoding.UTF8, "application/json")
            );

            if (!createResponse.IsSuccessStatusCode)
                return new RegisterResponseDto { Success = false, Message = "Erreur création compte Keycloak." };

            // --- Étape 2 : Sauvegarder dans SQL Server ---
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Department = request.Department,
                Role = request.RequestedRole,
                IsApproved = false, // En attente d'approbation admin
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new RegisterResponseDto
            {
                Success = true,
                Message = "Compte créé. En attente d'approbation par l'administrateur."
            };
        }

        /// <summary>
        /// Obtient un token admin Keycloak pour les opérations d'administration
        /// </summary>
        private async Task<string?> GetAdminTokenAsync()
{
    var tokenUrl = "http://localhost:8080/realms/master/protocol/openid-connect/token";

    var formData = new Dictionary<string, string>
    {
        { "grant_type", "password" },
        { "client_id", "admin-cli" },
        { "username", _config["Keycloak:AdminUsername"]! },  // ← depuis config
        { "password", _config["Keycloak:AdminPassword"]! }   // ← depuis config
    };

    var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(formData));
    if (!response.IsSuccessStatusCode) return null;

    var json = await response.Content.ReadAsStringAsync();
    var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(json,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    return tokenResponse?.access_token;
}

        // Classe interne pour désérialiser la réponse de Keycloak
        private class KeycloakTokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public string refresh_token { get; set; } = string.Empty;
            public int expires_in { get; set; }
        }
    }
}