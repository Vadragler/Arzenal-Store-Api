using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.TagDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Tags")]
    [Authorize(Policy = "ArzenalStoreManager")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Retourne tous les tags.
        /// </summary>
        /// <returns>
        /// - Une liste d'objets <c>ReadTagDto</c> représentant les tags.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ReadTagDto>>> GetAllTags()
        {
            var tag = await _tagService.GetAllAsync();
            return Ok(tag);
        }
        /// <summary>
        /// Retourne un tag par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du tag à récupérer.</param>
        /// <returns>
        /// - Un objet <c>ReadTagDto</c> représentant le tag demandé.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadTagDto>> GetTagById(Guid id)
        {
            var tag = await _tagService.GetByIdAsync(id);
            return Ok(tag);
        }

        /// <summary>
        /// Crée un nouveau tag.
        /// </summary>
        /// <param name="tagDto">Nom du tag à créer.</param>
        /// <returns>
        /// - Un objet <c>ReadTagDto</c> représentant le tag créé.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadTagDto>> PostTag(CreateTagDto tagDto)
        {
            var tagReadDto = await _tagService.CreateAsync(tagDto);
            return CreatedAtAction(nameof(GetTagById), new { id = tagReadDto.Id }, tagReadDto);
        }

        /// <summary>
        /// Met à jour un tag existant.
        /// </summary>
        /// <param name="id">Identifiant du tag à mettre à jour.</param>
        /// <param name="tagDto">Nom du tag à mettre à jour.</param>
        /// <returns>
        /// - Un code de statut HTTP 204 No Content en cas de succès.
        /// </returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ReadTagDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutTag(Guid id, UpdateTagDto tagDto)
        {
            await _tagService.UpdateAsync(id, tagDto);
            return NoContent();
        }

        /// <summary>
        /// Supprime un tag par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du tag à supprimer.</param>
        /// <returns>
        /// - Une réponse <c>204 NoContent</c> en cas de succès.
        /// </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            await _tagService.DeleteAsync(id);
            return NoContent();
        }
    }
}
