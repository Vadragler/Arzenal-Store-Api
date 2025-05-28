using ArzenalStoreApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Arzenal.Shared.Dtos.DTOs.OperatingSystemDto;

namespace ArzenalStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OperatingSystemsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadOperatingSystemDto>>> GetAllOS()
        {
            var os = await _context.OperatingSystems
                .Select(o => new ReadOperatingSystemDto
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToListAsync();

            return Ok(os);
        }

        // GET: api/OS/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadOperatingSystemDto>> GetOSByID(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var os = await _context.OperatingSystems
                .Where(o => o.Id == id)
                .Select(o => new ReadOperatingSystemDto
                {
                    Id = o.Id,
                    Name = o.Name
                }).FirstOrDefaultAsync();

            if (os == null)
            {
                return NotFound();
            }

            return Ok(os);
        }

        // POST: api/OS
        [HttpPost]
        public async Task<ActionResult<ReadOperatingSystemDto>> PostOS(CreateOperatingSystemDto osDto)
        {
            if (await _context.OperatingSystems.AnyAsync(o => o.Name == osDto.Name))
            {
                return BadRequest("Cette OS existe déjà");
            }
            var os = new Models.OperatingSystem
            {
                Name = osDto.Name!
            };

            _context.OperatingSystems.Add(os);
            await _context.SaveChangesAsync();

            // Utilisation de CreatedAtAction pour retourner la ressource créée avec son ID
            var osReadDto = new ReadOperatingSystemDto
            {
                Id = os.Id,
                Name = os.Name
            };

            return CreatedAtAction(nameof(GetOSByID), new { id = osReadDto.Id }, osReadDto);
        }

        // PUT: api/os/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOS(int id, UpdateOperatingSystemDto osDto)
        {
            if(id <= 0)
            {
                return BadRequest();
            }

            if (await _context.OperatingSystems.AnyAsync(o => o.Name == osDto.Name))
            {
                return BadRequest("Cette OS existe déjà");
            }

            var os = await _context.OperatingSystems.FindAsync(id);
            if (os == null)
            {
                return NotFound();
            }

            os.Name = osDto.Name;

            _context.Entry(os).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Language/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOS(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var os = await _context.OperatingSystems.FindAsync(id);
            if (os == null)
            {
                return NotFound();
            }

            _context.OperatingSystems.Remove(os);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
