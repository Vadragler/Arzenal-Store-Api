using Arzenal.Shared.Dtos.DTOs.CategorieDto;
using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArzenalStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController(ApplicationDbContext context) : ControllerBase
    {

        private readonly ApplicationDbContext _context = context;

        // GET: api/<ValuesController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categorie>>> GetAll()
        {
            var categories = await _context.Categories
             .Select(c => new ReadCategorieDto
              {
                  Id = c.Id,
                  Name = c.Name
              }).ToListAsync();

            return Ok(categories);
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadCategorieDto>> GetCategorieById(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var categorie = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new ReadCategorieDto
                {
                    Id = c.Id,
                    Name = c.Name
                }).FirstOrDefaultAsync();

            if (categorie == null)
            {
                return NotFound();
            }

            return Ok(categorie);
        }

        // POST api/<ValuesController>
        [HttpPost]
        public async Task<ActionResult<ReadCategorieDto>> PostCategorie(CreateCategorieDto CategorieDto)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == CategorieDto.Name))
            {
                return BadRequest("Cette catégorie existe déjà");
            }
            var categorie = new Categorie
            {
                Name = CategorieDto.Name!
            };

            _context.Categories.Add(categorie);
            await _context.SaveChangesAsync();

            // Utilisation de CreatedAtAction pour retourner la ressource créée avec son ID
            var categorieReadDto = new ReadCategorieDto
            {
                Id = categorie.Id,
                Name = categorie.Name!
            };

            return CreatedAtAction(nameof(GetCategorieById), new { id = categorieReadDto.Id }, categorieReadDto);
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]

        public async Task<IActionResult> PutCategorie(int id, UpdateCategorieDto CategorieDto)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            if (await _context.Categories.AnyAsync(c => c.Name == CategorieDto.Name))
            {
                return BadRequest("Cette catégorie existe déjà");
            }
            var caategorie = await _context.Categories.FindAsync(id);

            if (caategorie == null)
            {
                return NotFound();
            }

            caategorie.Name = CategorieDto.Name!;

            _context.Entry(caategorie).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategorie(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var categorie = await _context.Categories.FindAsync(id);
            if (categorie == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(categorie);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
