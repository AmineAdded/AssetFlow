// ============================================================
// AssetFlow.Infrastructure / Services / EmployeService.cs
// Implémentation concrète du service Employé
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using AssetFlow.Domain.Entities;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Services
{
    public class EmployeService : IEmployeService
    {
        private readonly AppDbContext _context;

        public EmployeService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les équipements affectés à un employé
        /// </summary>
        public async Task<List<EquipementAffecteDto>> GetEquipementsAffectesAsync(int utilisateurId)
        {
            var affectations = await _context.Affectations
                .Include(a => a.Materiel)
                .Where(a => a.UtilisateurId == utilisateurId)
                .OrderByDescending(a => a.DateAffectation)
                .ToListAsync();

            return affectations.Select(a => new EquipementAffecteDto
            {
                AffectationId = a.Id,
                MaterielId = a.MaterielId,
                Reference = a.Materiel.Reference,
                Designation = a.Materiel.Designation,
                Categorie = a.Materiel.Categorie,
                ImageUrl = a.Materiel.ImageUrl,
                DateAffectation = a.DateAffectation,
                QuantiteAffectee = a.QuantiteAffectee,
                Statut = a.Statut.ToString(),
                StatutBadgeColor = GetStatutColor(a.Statut),
                Observations = a.Observations
            }).ToList();
        }

        /// <summary>
        /// Récupère le détail d'une affectation
        /// </summary>
        public async Task<EquipementAffecteDto?> GetEquipementDetailAsync(int affectationId)
        {
            var affectation = await _context.Affectations
                .Include(a => a.Materiel)
                .FirstOrDefaultAsync(a => a.Id == affectationId);

            if (affectation == null)
                return null;

            return new EquipementAffecteDto
            {
                AffectationId = affectation.Id,
                MaterielId = affectation.MaterielId,
                Reference = affectation.Materiel.Reference,
                Designation = affectation.Materiel.Designation,
                Categorie = affectation.Materiel.Categorie,
                ImageUrl = affectation.Materiel.ImageUrl,
                DateAffectation = affectation.DateAffectation,
                QuantiteAffectee = affectation.QuantiteAffectee,
                Statut = affectation.Statut.ToString(),
                StatutBadgeColor = GetStatutColor(affectation.Statut),
                Observations = affectation.Observations
            };
        }

        /// <summary>
        /// Signale un incident sur un équipement
        /// NOTE: Pour l'instant, on met à jour juste les observations
        /// Dans une version complète, créer une table Incidents séparée
        /// </summary>
        public async Task<SignalerIncidentResponseDto> SignalerIncidentAsync(SignalerIncidentDto request)
        {
            var affectation = await _context.Affectations.FindAsync(request.AffectationId);

            if (affectation == null)
            {
                return new SignalerIncidentResponseDto
                {
                    Success = false,
                    Message = "Affectation introuvable."
                };
            }

            // Mettre à jour les observations avec l'incident
            var incidentLog = $"[{request.DateIncident:dd/MM/yyyy HH:mm}] {request.TypeIncident}: {request.Description}";
            affectation.Observations = string.IsNullOrEmpty(affectation.Observations)
                ? incidentLog
                : $"{affectation.Observations}\n{incidentLog}";

            // Si incident = Perte ou Dommage, changer le statut
            if (request.TypeIncident.ToLower().Contains("perte"))
                affectation.Statut = StatutAffectation.Perdu;
            else if (request.TypeIncident.ToLower().Contains("dommage") || request.TypeIncident.ToLower().Contains("panne"))
                affectation.Statut = StatutAffectation.Endommage;

            await _context.SaveChangesAsync();

            return new SignalerIncidentResponseDto
            {
                Success = true,
                Message = "Incident signalé avec succès. L'équipe IT a été notifiée.",
                IncidentId = affectation.Id
            };
        }

        /// <summary>
        /// Détermine la couleur du badge selon le statut
        /// </summary>
        private string GetStatutColor(StatutAffectation statut)
        {
            return statut switch
            {
                StatutAffectation.EnCours => "#10B981",    // Vert
                StatutAffectation.Retourne => "#94A3B8",   // Gris
                StatutAffectation.Perdu => "#EF4444",      // Rouge
                StatutAffectation.Endommage => "#F59E0B",  // Orange
                _ => "#6B7280"
            };
        }
    }
}