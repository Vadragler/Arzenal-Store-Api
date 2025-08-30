using ArzenalStoreApi.Data;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreApi.Models;
using ArzenalStoreSharedDto.DTOs.LanguageDto;
using Microsoft.EntityFrameworkCore;

namespace ArzenalStoreApi.Services.LanguageService
{
    
    public class LanguageService : ILanguageService
    {
        private readonly ApplicationDbContext _context;

        public LanguageService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<ReadLanguageDto>> GetAllAsync()
        {
            var languages = await _context.Set<Language>()
                .Select(l => new ReadLanguageDto { Id = l.Id, Name = l.Name })
                .ToListAsync();
            if (languages == null || languages.Count == 0)
            {
                throw new NotFoundException("Language introuvable");
            }
            return languages;
        }

        public async Task<ReadLanguageDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Id invalide");
            }
            var language = await _context.Set<Language>()
                .Where(l => l.Id == id)
                .Select(l => new ReadLanguageDto { Id = l.Id, Name = l.Name })
                .FirstOrDefaultAsync();
            if (language == null)
            {
                throw new NotFoundException("Language introuvable");
            }
            return language;
        }

        public async Task<ReadLanguageDto> CreateAsync(CreateLanguageDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom de la langue est requis");

            if (await _context.Set<Language>().AnyAsync(l => l.Name == dto.Name))
                throw new DuplicateException("Cette langue éxiste déjà");

            var language = new Language { Name = dto.Name };
            _context.Set<Language>().Add(language);
            await _context.SaveChangesAsync();
            return new ReadLanguageDto { Id = language.Id, Name = language.Name };
        }

        public async Task<bool> UpdateAsync(int id, UpdateLanguageDto dto)
        {
            if (id <= 0)
                throw new ValidationException("Id invalide");
            var language = await _context.Set<Language>().FindAsync(id);
            if (language == null)
                throw new NotFoundException("Language introuvable");
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom de la langue est requis");
            if (await _context.Set<Language>().AnyAsync(l => l.Name == dto.Name && l.Id != id))
                throw new DuplicateException("Cette langue éxiste déjà");
            language.Name = dto.Name;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("Id invalide");
            var language = await _context.Set<Language>().FindAsync(id);
            if (language == null)
                throw new NotFoundException("Language introuvable");
            _context.Set<Language>().Remove(language);
            await _context.SaveChangesAsync();
            return true;
        }
    }
    
}
