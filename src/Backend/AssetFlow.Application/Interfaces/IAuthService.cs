// ============================================================
// AssetFlow.Application / Interfaces / IAuthService.cs
// Interface du service d'authentification (contrat)
// L'implémentation concrète est dans Infrastructure
// ============================================================

using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Connecte un utilisateur via Keycloak et retourne les tokens
        /// </summary>
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// Inscrit un nouvel utilisateur dans Keycloak ET dans SQL Server
        /// </summary>
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    }
}