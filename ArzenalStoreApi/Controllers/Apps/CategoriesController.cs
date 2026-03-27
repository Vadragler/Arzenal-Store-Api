using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.CategorieDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Categories")]
    [Authorize(Policy = "ArzenalStoreManager")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Retourne toutes les catégories.
        /// </summary>
        /// <returns>
        /// - Une liste d'objets <c>ReadCategorieDto</c> représentant les catégories.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ReadCategorieDto>>> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Retourne une catégorie par son identifiant.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// - Un objet <c>ReadCategorieDto</c> représentant la catégorie demandée.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadCategorieDto>> GetById(Guid id)
        {
            return Ok(await _categoryService.GetByIdAsync(id));
        }

        /// <summary>
        /// Crée une nouvelle catégorie.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>
        /// - Un objet <c>ReadCategorieDto</c> représentant la catégorie créée.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(ReadCategorieDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadCategorieDto>> PostCategorie(CreateCategorieDto dto)
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Mets à jour une catégorie existante.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns>
        /// Un code de statut HTTP 204 (No Content) si la mise à jour est réussie.
        /// </returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutCategorie(Guid id, UpdateCategorieDto dto)
        {
            await _categoryService.UpdateAsync(id, dto);
            return NoContent();
        }

        /// <summary>
        /// Supprime une catégorie par son identifiant.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Un code de statut HTTP 204 (No Content) si la suppression est réussie.
        /// </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCategorie(Guid id)
        {
            await _categoryService.DeleteAsync(id);
            return NoContent(); 
        }
    }
}
