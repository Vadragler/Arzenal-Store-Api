using Microsoft.AspNetCore.Mvc;
using ArzenalApi.Models;
using Microsoft.EntityFrameworkCore;
using ArzenalApi.Data;
using Microsoft.AspNetCore.Authorization;
using Arzenal.Shared.Dtos.DTOs.LanguageDto;

namespace ArzenalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LanguagesController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // GET: api/Language
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadLanguageDto>>> GetAllLanguages()
        {
            var languages = await _context.Languages
                .Select(l => new ReadLanguageDto
                {
                    Id = l.Id,
                    Name = l.Name
                }).ToListAsync();

            return Ok(languages);
        }

        // GET: api/Language/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadLanguageDto>> GetLanguageById(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var language = await _context.Languages
                .Where(l => l.Id == id)
                .Select(l => new ReadLanguageDto
                {
                    Id = l.Id,
                    Name = l.Name
                }).FirstOrDefaultAsync();

            if (language == null)
            {
                return NotFound();
            }

            return Ok(language);
        }

        // POST: api/Language
        [HttpPost]
        public async Task<ActionResult<ReadLanguageDto>> PostLanguage(CreateLanguageDto languageDto)
        {
            if (await _context.Languages.AnyAsync(l => l.Name == languageDto.Name))
            {
                return BadRequest("Cette langue existe déjà");
            }
            var language = new Language
            {
                Name = languageDto.Name!
            };

            _context.Languages.Add(language);
            await _context.SaveChangesAsync();

            // Utilisation de CreatedAtAction pour retourner la ressource créée avec son ID
            var languageReadDto = new ReadLanguageDto
            {
                Id = language.Id,
                Name = language.Name
            };

            return CreatedAtAction(nameof(GetLanguageById), new { id = languageReadDto.Id }, languageReadDto);
        }

        // PUT: api/Language/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLanguage(int id, UpdateLanguageDto languageDto)
        {
            if(id <= 0)
            {
                return BadRequest();
            }

            if (await _context.Languages.AnyAsync(l => l.Name == languageDto.Name))
            {
                return BadRequest("Cette langue existe déjà");
            }

            var language = await _context.Languages.FindAsync(id);
       
            if (language == null)
            {
                return NotFound();
            }

            language.Name = languageDto.Name!;

            _context.Entry(language).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Language/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLanguage(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var language = await _context.Languages.FindAsync(id);
            if (language == null)
            {
                return NotFound();
            }

            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
