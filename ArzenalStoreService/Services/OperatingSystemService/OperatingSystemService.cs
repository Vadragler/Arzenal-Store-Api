using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreInfrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Mapping.OperatingSystemMapping;
using Arzenal.Dto.DTOs.OperatingSystemDto;
using Microsoft.EntityFrameworkCore;


namespace Arzenal.Store.Api.Service.Services.OperatingSystemService
{
    public class OperatingSystemService : IOperatingSystemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOperatingSystemMapper _mapper;
        public OperatingSystemService(ApplicationDbContext context,IOperatingSystemMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ReadOperatingSystemDto>> GetAllAsync()
        {
            var os = await _context.OperatingSystems
                .ToListAsync();
            return [.. os.Select(a => _mapper.ToReadOperatingSystemDto(a))];
        }

        public async Task<ReadOperatingSystemDto> GetByIdAsync(Guid id)
        {
            var os = await _context.OperatingSystems
                .FirstOrDefaultAsync(o => o.Id == id);
            if (os == null)
            {
                throw new NotFoundException("OS introuvable");
            }
            return _mapper.ToReadOperatingSystemDto(os);
        }

        public async Task<ReadOperatingSystemDto> CreateAsync(CreateOperatingSystemDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom de l'OS est requis");

            if (await _context.OperatingSystems.AnyAsync(o => o.Name == dto.Name))
            {
                throw new DuplicateException("Cette OS existe déjà");
            }
            var os = _mapper.ToOperatingSystem(dto);
            _context.OperatingSystems.Add(os);
            await _context.SaveChangesAsync();
            return _mapper.ToReadOperatingSystemDto(os);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateOperatingSystemDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom de l'OS est requis");

            var os = await _context.OperatingSystems.FindAsync(id);
            if (os == null)
                throw new NotFoundException("OS introuvable");
            
            if (await _context.OperatingSystems.AnyAsync(o => o.Name == dto.Name && o.Id != id))
                throw new DuplicateException("Cette OS existe déjà");
            
            _mapper.UpdateOperatingSystemFromDto(dto, os);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var os = await _context.OperatingSystems.FindAsync(id);
            if (os == null)
                throw new NotFoundException("OS introuvable");

            _context.OperatingSystems.Remove(os);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
