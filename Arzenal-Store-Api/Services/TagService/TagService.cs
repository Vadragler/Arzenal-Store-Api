using ArzenalStoreApi.Data;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreSharedDto.DTOs.TagDto;
using Microsoft.EntityFrameworkCore;

namespace ArzenalStoreApi.Services.TagService
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;
        public TagService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReadTagDto>> GetAllAsync()
        {
            var tags = await _context.Tags
                .Select(t => new ReadTagDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToListAsync();
            if (tags == null || tags.Count == 0)
            {
                throw new NotFoundException("Tag introuvable");
            }
            return tags;
        }

        public async Task<ReadTagDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Id invalide");
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
                throw new NotFoundException("Tag introuvable");
            }
            return tag;
        }

        public async Task<ReadTagDto> CreateAsync(CreateTagDto dto)
        {
            if (await _context.Tags.AnyAsync(t => t.Name == dto.Name))
            {
                throw new DuplicateException("Ce Tag existe déjà");
            }
            var tag = new Models.Tag
            {
                Name = dto.Name
            };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return new ReadTagDto
            {
                Id = tag.Id,
                Name = tag.Name
            };
        }

        public async Task<ReadTagDto> UpdateAsync(int id, UpdateTagDto dto)
        {
            if (id <= 0)
                throw new ValidationException("Id invalide");
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                throw new NotFoundException("Tag introuvable");
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom du tag est requis");
            if (await _context.Tags.AnyAsync(t => t.Name == dto.Name && t.Id != id))
                throw new DuplicateException("Ce tag éxiste déjà");
            tag.Name = dto.Name;
            await _context.SaveChangesAsync();
            return new ReadTagDto
            {
                Id = tag.Id,
                Name = tag.Name
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("Id invalide");
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                throw new NotFoundException("Tag introuvable");
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
