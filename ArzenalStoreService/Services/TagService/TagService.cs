using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreInfrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Mapping.TagMapping;
using Arzenal.Dto.DTOs.TagDto;
using Microsoft.EntityFrameworkCore;

namespace Arzenal.Store.Api.Service.Services.TagService
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITagMapper _mapper;
        public TagService(ApplicationDbContext context, ITagMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ReadTagDto>> GetAllAsync()
        {
            var tags = await _context.Tags
                .ToListAsync();

            return [.. tags.Select(a => _mapper.ToReadTagDto(a))];
        }

        public async Task<ReadTagDto> GetByIdAsync(Guid id)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
            {
                throw new NotFoundException("Tag introuvable");
            }
            return _mapper.ToReadTagDto(tag);
        }

        public async Task<ReadTagDto> CreateAsync(CreateTagDto dto)
        {
            if (await _context.Tags.AnyAsync(t => t.Name == dto.Name))
            {
                throw new DuplicateException("Ce Tag existe déjà");
            }

            var tag = _mapper.ToTag(dto);
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return _mapper.ToReadTagDto(tag);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTagDto dto)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                throw new NotFoundException("Tag introuvable");

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom du tag est requis");

            if (await _context.Tags.AnyAsync(t => t.Name == dto.Name && t.Id != id))
                throw new DuplicateException("Ce tag éxiste déjà");

            _mapper.UpdateTagFromDto(dto, tag);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                throw new NotFoundException("Tag introuvable");

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
