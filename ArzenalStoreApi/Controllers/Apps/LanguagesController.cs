using Arzenal.Dto.DTOs.LanguageDto;
using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Langues")]
    [Authorize(Policy = "ArzenalStoreManager")]
    public class LanguagesController : ControllerBase
    {
        private readonly ILanguageService _languageService;

        public LanguagesController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        /// <summary>
        /// Retourne toutes les langues.
        /// </summary>
        /// <returns>
        /// - Une liste d'objets <c>ReadLanguageDto</c> représentant les langues.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ReadLanguageDto>>> GetAllLanguages()
        {
            var languages = await _languageService.GetAllAsync();
            return Ok(languages);
        }

        /// <summary>
        /// Retourne une langue par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant de la langue à récupérer.</param>
        /// <returns>
        /// - Un objet <c>ReadLanguageDto</c> représentant la langue demandée.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadLanguageDto>> GetLanguageById(Guid id)
        {
            var language = await _languageService.GetByIdAsync(id);
            return Ok(language);
        }

        /// <summary>
        /// Crée une nouvelle langue.
        /// </summary>
        /// <param name="languageDto">Nom de la langue à créer</param>
        /// <returns>
        /// - Un objet <c>ReadLanguageDto</c> représentant la langue créée.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(ReadLanguageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadLanguageDto>> PostLanguage(CreateLanguageDto languageDto)
        {
            var languageCreated = await _languageService.CreateAsync(languageDto);
            return CreatedAtAction(nameof(GetLanguageById), new { id = languageCreated!.Id }, languageCreated);
        }

        /// <summary>
        /// Met à jour une langue existante.
        /// </summary>
        /// <param name="id">Identifiant de la langue à mettre à jour.</param>
        /// <param name="languageDto">Nom de la langue à mettre à jour.</param>
        /// <returns>
        /// - Un code de statut HTTP 204 (No Content) indiquant que la mise à jour a réussi.
        /// </returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutLanguage(Guid id, UpdateLanguageDto languageDto)
        {
            await _languageService.UpdateAsync(id,languageDto);
            return NoContent();
        }

        /// <summary>
        /// Suprime une langue par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant de la langue à supprimer.</param>
        /// <returns>
        /// - Une réponse <c>204 NoContent</c> en cas de succès.
        /// </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteLanguage(Guid id)
        {
            await _languageService.DeleteAsync(id);
            return NoContent();
        }
    }
}
