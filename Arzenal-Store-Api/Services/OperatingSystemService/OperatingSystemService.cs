using ArzenalStoreApi.Data;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreSharedDto.DTOs.OperatingSystemDto;
using Microsoft.EntityFrameworkCore;
using OperatingSystem = ArzenalStoreApi.Models.OperatingSystem;

namespace ArzenalStoreApi.Services.OperatingSystemService
{
    public class OperatingSystemService : IOperatingSystemService
    {
        private readonly ApplicationDbContext _context;
        public OperatingSystemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReadOperatingSystemDto>> GetAllAsync()
        {

            var os = await _context.OperatingSystems
                .Select(o => new ReadOperatingSystemDto
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToListAsync();
            if (os == null || os.Count == 0)
            {
                throw new NotFoundException("OS introuvable");
            }
            return os;
        }

        public async Task<ReadOperatingSystemDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Id invalide");
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
                throw new NotFoundException("OS introuvable");
            }
            return os;
        }

        public async Task<ReadOperatingSystemDto> CreateAsync(CreateOperatingSystemDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom de l'OS est requis");

            if (await _context.OperatingSystems.AnyAsync(o => o.Name == dto.Name))
            {
                throw new DuplicateException("Cette OS existe déjà");
            }
            var os = new OperatingSystem
            {
                Name = dto.Name
            };
            _context.OperatingSystems.Add(os);
            await _context.SaveChangesAsync();
            return new ReadOperatingSystemDto
            {
                Id = os.Id,
                Name = os.Name
            };
        }

        public async Task<ReadOperatingSystemDto> UpdateAsync(int id, UpdateOperatingSystemDto dto)
        {
            if (id <= 0)
            {
                throw new ValidationException("Id invalide");
            }
            var os = await _context.OperatingSystems.FindAsync(id);
            if (os == null)
            {
                throw new NotFoundException("OS introuvable");
            }
            if (await _context.OperatingSystems.AnyAsync(o => o.Name == dto.Name && o.Id != id))
            {
                throw new DuplicateException("Cette OS existe déjà");
            }
            os.Name = dto.Name;
            await _context.SaveChangesAsync();
            return new ReadOperatingSystemDto
            {
                Id = os.Id,
                Name = os.Name
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Id invalide");
            }
            var os = await _context.OperatingSystems.FindAsync(id);
            if (os == null)
            {
                throw new NotFoundException("OS introuvable");
            }
            _context.OperatingSystems.Remove(os);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
