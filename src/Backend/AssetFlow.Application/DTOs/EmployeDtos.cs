// ============================================================
// AssetFlow.Application / DTOs / EmployeDtos.cs
// DTOs pour les opérations de l'employé
// ============================================================

namespace AssetFlow.Application.DTOs
{
    /// <summary>
    /// DTO représentant un équipement affecté à un employé
    /// (Version simplifiée pour l'affichage dans la liste)
    /// </summary>
    public class EquipementAffecteDto
    {
        /// <summary>ID de l'affectation</summary>
        public int AffectationId { get; set; }

        /// <summary>ID du matériel</summary>
        public int MaterielId { get; set; }

        /// <summary>Référence du matériel (ex: SN-5592-X)</summary>
        public string Reference { get; set; } = string.Empty;

        /// <summary>Nom du matériel (ex: Laptop Dell Latitude)</summary>
        public string Designation { get; set; } = string.Empty;

        /// <summary>Catégorie (ex: Ordinateur, Casque, Écran)</summary>
        public string Categorie { get; set; } = string.Empty;

        /// <summary>URL de l'image du matériel</summary>
        public string? ImageUrl { get; set; }

        /// <summary>Date d'affectation</summary>
        public DateTime DateAffectation { get; set; }

        /// <summary>Quantité affectée</summary>
        public int QuantiteAffectee { get; set; }

        /// <summary>Statut de l'affectation (EnCours, Retourne, etc.)</summary>
        public string Statut { get; set; } = string.Empty;

        /// <summary>Badge de couleur selon le statut (pour l'UI)</summary>
        public string StatutBadgeColor { get; set; } = string.Empty;

        /// <summary>Observations éventuelles</summary>
        public string? Observations { get; set; }
    }

    /// <summary>
    /// DTO pour signaler un incident sur un équipement
    /// </summary>
    public class SignalerIncidentDto
    {
        /// <summary>ID de l'affectation concernée</summary>
        public int AffectationId { get; set; }

        /// <summary>Type d'incident (Panne, Perte, Dommage, Autre)</summary>
        public string TypeIncident { get; set; } = string.Empty;

        /// <summary>Description de l'incident</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Date de l'incident</summary>
        public DateTime DateIncident { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Réponse après signalement d'incident
    /// </summary>
    public class SignalerIncidentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? IncidentId { get; set; }
    }
}