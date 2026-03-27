using Arzenal.Dto.DTOs.AccountDto;
using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ArzenalStoreApi.Controllers.User
{
    [Authorize(Policy = "Web")]
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Utilisateur")]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Recupère les informations de l'utilisateur actuellement connecté
        /// </summary>
        /// <returns>
        /// 200 OK avec un objet <c>ReadAccountDto</c> représentant l'utilisateur actuel.
        /// </returns>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ReadAccountDto user = await _userService.FindByIdAsync(userId);
            return Ok(user);
        }

        /// <summary>
        /// Supprime le compte de l'utilisateur actuellement connecté
        /// </summary>
        /// <returns>
        /// 204 No Content si la suppression est réussie.
        /// </returns>
        [HttpDelete("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _userService.DeleteAsync(userId);
            return NoContent();
        }

        /// <summary>
        /// Met à jour partiellement les informations de l'utilisateur actuellement connecté
        /// </summary>
        /// <param name="updateaccountdto">Objet contenant les champs à mettre à jour pour l'utilisateur.</param>
        /// <returns>
        /// 204 No Content si la mise à jour est réussie.
        /// </returns>
        [HttpPatch("me")]
        public async Task<IActionResult> PatchCurrentUser([FromBody] UpdateAccountDto updateaccountdto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _userService.PatchAsync(userId, updateaccountdto);
            return NoContent();
        }
    }
}
