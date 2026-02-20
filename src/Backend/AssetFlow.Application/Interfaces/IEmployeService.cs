// ============================================================
// AssetFlow.Application / Interfaces / IEmployeService.cs
// Interface du service Employé (contrat)
// Implémentation dans Infrastructure
// ============================================================

using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    /// <summary>
    /// Service pour les opérations de l'employé
    /// </summary>
    public interface IEmployeService
    {
        /// <summary>
        /// Récupère tous les équipements affectés à un employé donné
        /// </summary>
        /// <param name="utilisateurId">ID de l'employé</param>
        /// <returns>Liste des équipements affectés</returns>
        Task<List<EquipementAffecteDto>> GetEquipementsAffectesAsync(int utilisateurId);

        /// <summary>
        /// Récupère le détail d'une affectation spécifique
        /// </summary>
        /// <param name="affectationId">ID de l'affectation</param>
        /// <returns>Détail de l'équipement affecté</returns>
        Task<EquipementAffecteDto?> GetEquipementDetailAsync(int affectationId);

        /// <summary>
        /// Permet à l'employé de signaler un incident sur un équipement
        /// </summary>
        /// <param name="request">Données de l'incident</param>
        /// <returns>Confirmation du signalement</returns>
        Task<SignalerIncidentResponseDto> SignalerIncidentAsync(SignalerIncidentDto request);
    }
}