using ArzenalStoreApi.Data;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreApi.Models;
using ArzenalStoreSharedDto.DTOs.CategorieDto;
using Microsoft.EntityFrameworkCore;

namespace ArzenalStoreApi.Services.CategoryService
{
    

    
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReadCategorieDto>> GetAllAsync()
        {
            
            var Categorie = await _context.Categories
                .Select(c => new ReadCategorieDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
            if (Categorie == null || Categorie.Count == 0)
                throw new NotFoundException("Categorie introuvable");
            return Categorie;
        }

        public async Task<ReadCategorieDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Id invalide");
            }
            else { 
            var categorie = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new ReadCategorieDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .FirstOrDefaultAsync();
                if(categorie == null) throw new NotFoundException("Categorie introuvable");
                return categorie;
            }
        }

        public async Task<ReadCategorieDto?> CreateAsync(CreateCategorieDto dto)
        {

            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
                throw new DuplicateException("Cette categorie éxiste déjà");

            var categorie = new Categorie { Name = dto.Name! };
            
                _context.Categories.Add(categorie);
                await _context.SaveChangesAsync();
           

            return new ReadCategorieDto { Id = categorie.Id, Name = categorie.Name! };
        }

        public async Task<bool> UpdateAsync(int id, UpdateCategorieDto dto)
        {
            if (id <= 0) throw new ValidationException("Id invalide");
            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
                throw new DuplicateException("Cette categorie éxiste déjà");

            var categorie = await _context.Categories.FindAsync(id);
            if (categorie == null) throw new NotFoundException("Categorie introuvable");

            categorie.Name = dto.Name!;
           
                _context.Entry(categorie).State = EntityState.Modified;
                await _context.SaveChangesAsync();
           
            
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0) throw new ValidationException("Id invalide");
            var categorie = await _context.Categories.FindAsync(id);
            if (categorie == null) throw new NotFoundException("Catégorie introuvable");

            _context.Categories.Remove(categorie);
            await _context.SaveChangesAsync();

            return true;
        }
    }
    
}
