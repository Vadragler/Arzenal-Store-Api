using Microsoft.AspNetCore.Mvc;
using ArzenalStoreApi.Models;
using Microsoft.EntityFrameworkCore;
using ArzenalStoreApi.Data;
using Microsoft.AspNetCore.Authorization;
using ArzenalStoreSharedDto.DTOs.LanguageDto;
using ArzenalStoreApi.Services.LanguageService;

namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LanguagesController(ILanguageService languageService) : ControllerBase
    {
        private readonly ILanguageService _languageService = languageService;

        // GET: api/Language
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadLanguageDto>>> GetAllLanguages()
        {
            var languages = await _languageService.GetAllAsync();
            return Ok(languages);
        }

        // GET: api/Language/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadLanguageDto>> GetLanguageById(int id)
        {
            var language = await _languageService.GetByIdAsync(id);
            return Ok(language);
        }

        // POST: api/Language
        [HttpPost]
        public async Task<ActionResult<ReadLanguageDto>> PostLanguage(CreateLanguageDto languageDto)
        {
            var languageCreated = await _languageService.CreateAsync(languageDto);
            return CreatedAtAction(nameof(GetLanguageById), new { id = languageCreated!.Id }, languageCreated);
        }

        // PUT: api/Language/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLanguage(int id, UpdateLanguageDto languageDto)
        {
            await _languageService.UpdateAsync(id,languageDto);
            return NoContent();
        }

        // DELETE: api/Language/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLanguage(int id)
        {
            await _languageService.DeleteAsync(id);
            return NoContent();
        }
    }
}
