// ============================================================
// AssetFlow.Application / Interfaces / IIncidentService.cs
// Interface du service Incident (contrat)
// ============================================================

using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    /// <summary>
    /// Service pour la gestion des incidents
    /// </summary>
    public interface IIncidentService
    {
        /// <summary>
        /// Signale un nouvel incident
        /// </summary>
        Task<SignalerIncidentResponseDto> SignalerIncidentAsync(SignalerIncidentRequestDto request);

        /// <summary>
        /// Récupère tous les incidents d'un utilisateur
        /// </summary>
        Task<List<IncidentDto>> GetIncidentsUtilisateurAsync(int utilisateurId);

        /// <summary>
        /// Récupère le détail d'un incident
        /// </summary>
        Task<IncidentDto?> GetIncidentDetailAsync(int incidentId);
    }
}