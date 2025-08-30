using ArzenalStoreApi.Services.OperatingSystemService;
using ArzenalStoreSharedDto.DTOs.OperatingSystemDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OperatingSystemsController(IOperatingSystemService operatingSystemService) : ControllerBase
    {
        private readonly IOperatingSystemService _operatingSystemService = operatingSystemService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadOperatingSystemDto>>> GetAllOS()
        {
            var os = await _operatingSystemService.GetAllAsync();
            return Ok(os);
        }

        // GET: api/OS/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadOperatingSystemDto>> GetOSByID(int id)
        {
            var os = await _operatingSystemService.GetByIdAsync(id);
            return Ok(os);
        }

        // POST: api/OS
        [HttpPost]
        public async Task<ActionResult<ReadOperatingSystemDto>> PostOS(CreateOperatingSystemDto osDto)
        {
            var osReadDto = await _operatingSystemService.CreateAsync(osDto);
            return CreatedAtAction(nameof(GetOSByID), new { id = osReadDto.Id }, osReadDto);
        }

        // PUT: api/os/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOS(int id, UpdateOperatingSystemDto osDto)
        {
            await _operatingSystemService.UpdateAsync(id, osDto);
            return NoContent();
        }

        // DELETE: api/Language/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOS(int id)
        {
            await _operatingSystemService.DeleteAsync(id);
            return NoContent();
        }
    }
}
