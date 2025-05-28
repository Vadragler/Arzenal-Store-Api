using Arzenal.Shared.Dtos.DTOs.TagDto;
using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArzenalStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TagsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadTagDto>>> GetAllTag()
        {
            var tag = await _context.Tags
                .Select(t => new ReadTagDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToListAsync();

            return Ok(tag);
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadTagDto>> GetTagById(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var tag = await _context.Tags
                .Where(t => t.Id == id)
                .Select(t => new ReadTagDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).FirstOrDefaultAsync();

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<ActionResult<ReadTagDto>> PostTag(CreateTagDto tagDto)
        {
            if (await _context.Tags.AnyAsync(t => t.Name == tagDto.Name))
            {
                return BadRequest("Ce tag existe déjà");
            }
            var tag = new Tag
            {
                Name = tagDto.Name
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            // Utilisation de CreatedAtAction pour retourner la ressource créée avec son ID
            var tagReadDto = new ReadTagDto
            {
                Id = tag.Id,
                Name = tag.Name
            };

            return CreatedAtAction(nameof(GetTagById), new { id = tagReadDto.Id }, tagReadDto);
        }

        // PUT: api/Tags/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTag(int id, UpdateTagDto tagDto)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            if (await _context.Tags.AnyAsync(t => t.Name == tagDto.Name))
            {
                return BadRequest("Ce tag existe déjà");
            }

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            tag.Name = tagDto.Name;

            _context.Entry(tag).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Language/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
