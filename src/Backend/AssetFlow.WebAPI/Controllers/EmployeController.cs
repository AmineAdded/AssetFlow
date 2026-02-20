// ============================================================
// AssetFlow.WebAPI / Controllers / EmployeController.cs
// Contrôleur API pour les opérations Employé
// Endpoints appelés par le frontend Blazor
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // À décommenter quand l'auth JWT est activée
    public class EmployeController : ControllerBase
    {
        private readonly IEmployeService _employeService;

        public EmployeController(IEmployeService employeService)
        {
            _employeService = employeService;
        }

        /// <summary>
        /// GET api/employe/equipements/{utilisateurId}
        /// Récupère tous les équipements affectés à un employé
        /// </summary>
        [HttpGet("equipements/{utilisateurId}")]
        public async Task<IActionResult> GetEquipementsAffectes(int utilisateurId)
        {
            if (utilisateurId <= 0)
                return BadRequest("ID utilisateur invalide.");

            var equipements = await _employeService.GetEquipementsAffectesAsync(utilisateurId);
            return Ok(equipements);
        }

        /// <summary>
        /// GET api/employe/equipements/detail/{affectationId}
        /// Récupère le détail d'une affectation
        /// </summary>
        [HttpGet("equipements/detail/{affectationId}")]
        public async Task<IActionResult> GetEquipementDetail(int affectationId)
        {
            var equipement = await _employeService.GetEquipementDetailAsync(affectationId);

            if (equipement == null)
                return NotFound("Affectation introuvable.");

            return Ok(equipement);
        }
  
    }
}