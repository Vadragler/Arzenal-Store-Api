using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreInfrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Mapping.CategoryMapping;
using Arzenal.Dto.DTOs.CategorieDto;
using Microsoft.EntityFrameworkCore;

namespace Arzenal.Store.Api.Service.Services.CategoryService
{ 
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoryMapper _mapper;

        public CategoryService(ApplicationDbContext context,ICategoryMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ReadCategorieDto>> GetAllAsync()
        {
            
            var categorie = await _context.Categories
                .ToListAsync();
            return [.. categorie.Select(a => _mapper.ToReadCategoryDto(a))];
        }

        public async Task<ReadCategorieDto?> GetByIdAsync(Guid id)
        {
            var categorie = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
                if(categorie == null) throw new NotFoundException("Categorie introuvable");
                return _mapper.ToReadCategoryDto(categorie);
            
        }

        public async Task<ReadCategorieDto?> CreateAsync(CreateCategorieDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Le nom de la categorie est requis");

            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
                throw new DuplicateException("Cette categorie éxiste déjà");

            var categorie = _mapper.ToCategory(dto);
            
                _context.Categories.Add(categorie);
                await _context.SaveChangesAsync();
           

            return _mapper.ToReadCategoryDto(categorie);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCategorieDto dto)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
                throw new DuplicateException("Cette categorie éxiste déjà");

            var categorie = await _context.Categories.FindAsync(id);
            if (categorie == null) throw new NotFoundException("Categorie introuvable");

            _mapper.UpdateCategoryFromDto(dto,categorie);
           
            _context.Entry(categorie).State = EntityState.Modified;
            await _context.SaveChangesAsync();
           
            
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var categorie = await _context.Categories.FindAsync(id);
            if (categorie == null) throw new NotFoundException("Catégorie introuvable");

            _context.Categories.Remove(categorie);
            await _context.SaveChangesAsync();

            return true;
        }
    }   
}
