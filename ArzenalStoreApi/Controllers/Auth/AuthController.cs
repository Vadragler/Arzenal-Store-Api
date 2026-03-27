using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArzenalStoreApi.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    [SwaggerTag("Authentification")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IAuthService _authService;
        private readonly IRequestInfoProvider _requestInfoProvider;

        public AuthController(IAuthService authService,IJwtService jwtService,IRequestInfoProvider requestinfoProvider)
        {
            _jwtService = jwtService;
            _authService = authService;
            _requestInfoProvider = requestinfoProvider;
        }

        /// <summary>
        /// Inscription d'un nouvel utilisateur
        /// </summary>
        /// <param name="request">Objet contenant les informations nécessaires pour l'inscription</param>
        /// <returns>
        /// 201 Created si l'inscription est réussie
        /// </returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            await _authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Connexion d'un utilisateur existant
        /// </summary>
        /// <param name="request">Objet contenant les informations nécessaires pour la connexion.</param>
        /// <returns>
        /// 200 OK si la connexion est réussie et envoie un message de succès
        /// </returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var infoProvider = await _requestInfoProvider.GetRequestInfo(HttpContext, request);

            await _authService.AuthenticateAsync(Response, request.Email, request.Password, infoProvider);

            return Ok(new { message = "Connexion réussie" });
        }

        /// <summary>
        /// Rafraîchissement du token d'accès à l'aide du token de rafraîchissement
        /// </summary>
        /// <returns>
        /// 200 OK si la connexion est réussie
        /// </returns>
        /// <remarks>
        /// Le nouveau token est retourné dans un cookie HTTP-only nommé "refreshToken".
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            await _authService.RotateRefreshTokenAsync(HttpContext);
            return Ok();
        }

        /// <summary>
        /// Génération d'un nouveau token de rafraîchissement
        /// </summary>
        /// <param name="dto">Objet contenant les informations nécessaires pour générer un token de rafraîchissement.</param>
        /// <returns>
        /// 200 OK si la génération est réussie
        /// </returns>
        [Authorize(Policy = "AppLogin")]
        [HttpPost("GenerateRefreshToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateRefreshToken([FromBody] CreateRefreshTokenDto dto)
        {
            await _authService.GenerateRefreshTokenAsync(HttpContext, dto);
            return Ok();
        }

        /// <summary>
        /// Révocation des tokens
        /// </summary>
        /// <returns>
        /// 200 OK si la révocation est réussie avec un objet contenant :
        /// - <c>Message</c> : "Token révoqué avec succès."
        /// </returns>
        [Authorize(Policy = "ArzenalAuth")]
        [HttpPost("revoke")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Revoke()
        {
            await _authService.RevokeRefreshTokenAsync(HttpContext);
            return Ok(new { Message = "Token révoqué avec succès." });
        }

        /// <summary>
        /// Déconnexion de l'utilisateur
        /// </summary>
        /// <returns>
        /// 200 OK si la déconnexion est réussie avec un objet contenant :
        /// - <c>Message</c> : "Déconnexion réussie"
        /// </returns>
        [Authorize(Policy = "ArzenalAuth")]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(HttpContext);
            return Ok(new { Message = "Déconnexion réussie" });
        }

        /// <summary>
        /// Point de terminaison de test pour vérifier si l'authentification fonctionne
        /// </summary>
        /// <returns>
        /// 200 OK si l'authentification est réussie avec un objet contenant :
        /// - <c>status</c> : "ok"
        /// </returns>
        [Authorize(Policy = "ArzenalAuth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "ok" });
        }

        [HttpGet("ArzenalStoreManager/access")]
        [Authorize(Policy = "ArzenalStoreManager")]
        public IActionResult CheckAccess()
        {
            return Ok(new
            {
                Access = true
            });
        }

        /// <summary>
        /// Renvoie un token JWT pour l'application WPF
        /// </summary>
        /// <returns>
        /// 200 OK avec un objet contenant :
        /// - <c>token</c> : le token JWT généré
        /// </returns>
        [Authorize(Policy = "ArzenalAuth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("create-app-login-token")]
        public async Task<IActionResult> Create_App_Login_Token()
        {
            var jwt = await _jwtService.GenerateJwtTokenForAppAsync(Request);
            return Ok(new { token = jwt });
        }
    }
}
