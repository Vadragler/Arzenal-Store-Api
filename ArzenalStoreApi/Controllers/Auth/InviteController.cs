using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArzenalStoreApi.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Invitation")]
    public class InviteController : ControllerBase
    {
        private readonly IInviteService _inviteService;

        public InviteController(IInviteService inviteService)
        {
            _inviteService = inviteService;
        }

        /// <summary>
        /// Récupère et valide un token d'invitation
        /// </summary>
        /// <param name="token">Le token d'invitation à valider</param>
        /// <returns>
        /// 200 OK si le token est valide
        /// </returns>
        [AllowAnonymous]
        [HttpGet("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidateToken([FromQuery] string token)
        {
            string tokenFromQuery = token.Trim();
            await _inviteService.ValidateInviteAsync(tokenFromQuery);
            return Ok(new { message = "Token valide" });
        }

        /// <summary>
        /// Génère un token d'invitation pour un email donné
        /// </summary>
        /// <param name="email">Adresse email pour laquelle générer un token d'invitation.</param>
        /// <returns>
        /// 200 OK avec un objet contenant:
        /// - <c>Link</c> : le lien d'invitation généré
        /// </returns>
        [Authorize(Policy = "Admin")]
        [HttpPost("generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateToken([FromBody] string email)
        {
            var invitelink = await _inviteService.CreateInviteAsync(email);
            return Ok(new { Link = invitelink });
        }

        /// <summary>
        /// Récupère et utilise un token d'invitation
        /// </summary>
        /// <param name="token">Le token d'invitation à utiliser.</param>
        /// <returns>
        /// 200 OK si le token est utilisé avec succès
        /// </returns>
        [AllowAnonymous]
        [HttpPost("use")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UseToken([FromQuery] string token)
        {
            await _inviteService.UseInviteAsync(token);
            return Ok(new { message = "Token utilisé" });
        }
    }
}
