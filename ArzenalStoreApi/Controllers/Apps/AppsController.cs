using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arzenal.Dto.DTOs.AppDto;
using Arzenal.Store.Api.Service.Interfaces;
using Swashbuckle.AspNetCore.Annotations;



namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Apps")]
    [Authorize(Policy = "ArzenalStoreManager")]
    public class AppsController : ControllerBase
    {
        private readonly IAppService _appService;
        public AppsController(IAppService appService)
        {
            _appService = appService;
        }

        /// <summary>
        /// Retourne toutes les applications.
        /// </summary>
        /// <returns>
        /// - Une liste d'objets <c>ReadAppDto</c> représentant les applications.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ReadAppDto>>> GetAllApps()
        {
            var appDtos = await _appService.GetAllAppsAsync();
            if (appDtos == null || !appDtos.Any())
            {
                return Ok(new { message = "La Liste est vide" });
            }
            return Ok(appDtos);
        }

        /// <summary>
        /// Retourne une application par son identifiant.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// - Un objet <c>ReadAppDto</c> représentant l'application demandée.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadAppDto>> GetAppById(Guid id)
        {
            var appDto = await _appService.GetAppByIdAsync(id);
            return Ok(appDto);
        }

        /// <summary>
        /// Crée une nouvelle application.
        /// </summary>
        /// <param name="appDto"></param>
        /// <returns>
        /// - Un objet <c>ReadAppDto</c> représentant l'application créée.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(ReadAppDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateApp([FromBody] CreateAppDto appDto)
        {
            var appCreated = await _appService.CreateAppAsync(appDto);
            return CreatedAtAction(nameof(GetAppById), new { id = appCreated.Id }, appCreated);
        }

        /// <summary>
        /// Met à jour une application existante.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateAppDto"></param>
        /// <returns>
        /// - Un objet <c>ReadAppDto</c> représentant l'application mise à jour.
        /// </returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateApp(Guid id, [FromBody] UpdateAppDto updateAppDto)
        {
            var result = await _appService.UpdateAppAsync(id, updateAppDto);
            return Ok(result);
        }

        /// <summary>
        /// Suprime une application par son identifiant.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Une réponse <c>204 NoContent</c> en cas de succès.
        /// </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteApp(Guid id)
        {
           await _appService.DeleteAppAsync(id);
           return NoContent();
        }
    }
}
