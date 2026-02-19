// ============================================================
// AssetFlow.WebAPI / Controllers / AuthController.cs
// Contrôleur API pour Login et Register
// Endpoints appelés par le frontend Blazor
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route: api/auth
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Injection du service via l'interface (pas la classe concrète)
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// POST api/auth/login
        /// Connecte un utilisateur et retourne le token JWT
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Validation basique des données reçues
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Email et mot de passe requis.");

            var result = await _authService.LoginAsync(request);

            // Si Keycloak n'a pas validé les credentials
            if (result == null)
                return Unauthorized("Email ou mot de passe incorrect.");

            // Retourner le token et les infos utilisateur
            return Ok(result);
        }

        /// <summary>
        /// POST api/auth/register
        /// Inscrit un nouvel utilisateur (dans Keycloak + SQL Server)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            // Validation basique
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Données incomplètes.");

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(result.Message);

            // 201 Created avec message de succès
            return Created("", result);
        }
    }
}