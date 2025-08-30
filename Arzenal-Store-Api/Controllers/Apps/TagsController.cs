using ArzenalStoreSharedDto.DTOs.TagDto;
using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArzenalStoreApi.Services.TagService;

namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TagsController(ITagService tagService) : ControllerBase
    {
        private readonly ITagService _tagService = tagService;

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadTagDto>>> GetAllTag()
        {
            var tag = await _tagService.GetAllAsync();
            return Ok(tag);
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadTagDto>> GetTagById(int id)
        {
            var tag = await _tagService.GetByIdAsync(id);
            return Ok(tag);
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<ActionResult<ReadTagDto>> PostTag(CreateTagDto tagDto)
        {
            var tagReadDto = await _tagService.CreateAsync(tagDto);
            return CreatedAtAction(nameof(GetTagById), new { id = tagReadDto.Id }, tagReadDto);
        }

        // PUT: api/Tags/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTag(int id, UpdateTagDto tagDto)
        {
            await _tagService.UpdateAsync(id, tagDto);
            return NoContent();
        }

        // DELETE: api/Language/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            await _tagService.DeleteAsync(id);
            return NoContent();
        }
    }
}
