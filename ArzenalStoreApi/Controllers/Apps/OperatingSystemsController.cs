using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.OperatingSystemDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Operating Systems")]
    [Authorize(Policy = "ArzenalStoreManager")]
    public class OperatingSystemsController : ControllerBase
    {
        private readonly IOperatingSystemService _operatingSystemService;

        public OperatingSystemsController(IOperatingSystemService operatingSystemService)
        {
            _operatingSystemService = operatingSystemService;
        }

        /// <summary>
        /// Retourne tous les systèmes d'exploitation.
        /// </summary>
        /// <returns>
        /// - Une liste d'objets <c>ReadOperatingSystemDto</c> représentant les systèmes d'exploitation.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ReadOperatingSystemDto>>> GetAllOperatingSystems()
        {
            var os = await _operatingSystemService.GetAllAsync();
            return Ok(os);
        }

        /// <summary>
        /// Retourne un système d'exploitation par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du system d'exploitation à récupérer.</param>
        /// <returns>
        /// - Un objet <c>ReadOperatingSystemDto</c> représentant le système d'exploitation demandé.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadOperatingSystemDto>> GetOperatingSystemById(Guid id)
        {
            var os = await _operatingSystemService.GetByIdAsync(id);
            return Ok(os);
        }

        /// <summary>
        /// Crée un nouveau système d'exploitation.
        /// </summary>
        /// <param name="osDto">Nom du system d'exploitation à créer</param>
        /// <returns>
        /// - Un objet <c>ReadOperatingSystemDto</c> représentant le système d'exploitation créé.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(ReadOperatingSystemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadOperatingSystemDto>> PostOperatingSystem(CreateOperatingSystemDto osDto)
        {
            var osReadDto = await _operatingSystemService.CreateAsync(osDto);
            return CreatedAtAction(nameof(GetOperatingSystemById), new { id = osReadDto.Id }, osReadDto);
        }

        /// <summary>
        /// Met à jour un système d'exploitation existant.
        /// </summary>
        /// <param name="id">Identifiant du system d'exploitation à mettre à jour.</param>
        /// <param name="osDto">Nom du system d'exploitation à mettre à jour.</param>
        /// <returns>
        /// - Un code de statut <c>204 No Content</c> indiquant que la mise à jour a réussi.
        /// </returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutOperatingSystem(Guid id, UpdateOperatingSystemDto osDto)
        {
            await _operatingSystemService.UpdateAsync(id, osDto);
            return NoContent();
        }

        /// <summary>
        /// Suprime un système d'exploitation par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du system d'exploitation à supprimer.</param>
        /// <returns>
        /// - Une réponse <c>204 NoContent</c> en cas de succès.
        /// </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteOperatingSystem(Guid id)
        {
            await _operatingSystemService.DeleteAsync(id);
            return NoContent();
        }
    }
}
