using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreInfrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Mapping.LanguageMapping;
using Arzenal.Dto.DTOs.LanguageDto;
using Microsoft.EntityFrameworkCore;
using Arzenal.Dto.DTOs.LanguageDto;

namespace Arzenal.Store.Api.Service.Services.LanguageService
{
    
    public class LanguageService : ILanguageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILanguageMapper _mapper;

        public LanguageService(ApplicationDbContext context,ILanguageMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ReadLanguageDto>> GetAllAsync()
        {
            var languages = await _context.Languages
                .ToListAsync();

            return [.. languages.Select(a => _mapper.ToReadLanguageDto(a))];
        }

        public async Task<ReadLanguageDto> GetByIdAsync(Guid id)
        {
            var language = await _context.Languages
                .FirstOrDefaultAsync(l => l.Id == id);
            if (language == null)
            {
                throw new NotFoundException("Language introuvable");
            }

            return _mapper.ToReadLanguageDto(language);
        }

        public async Task<ReadLanguageDto> CreateAsync(CreateLanguageDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom de la langue est requis");

            if (await _context.Languages.AnyAsync(l => l.Name == dto.Name))
                throw new DuplicateException("Cette langue éxiste déjà");

            var language = _mapper.ToLanguage(dto);
            _context.Languages.Add(language);
            await _context.SaveChangesAsync();

            return _mapper.ToReadLanguageDto(language);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateLanguageDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom de la langue est requis");

            var language = await _context.Languages.FindAsync(id);

            if (language == null)
                throw new NotFoundException("Language introuvable");

            if (await _context.Languages.AnyAsync(l => l.Name == dto.Name && l.Id != id))
                throw new DuplicateException("Cette langue éxiste déjà");

            _mapper.UpdateLanguageFromDto(dto,language);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var language = await _context.Languages.FindAsync(id);

            if (language == null)
                throw new NotFoundException("Language introuvable");

            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();

            return true;
        }
    }  
}
