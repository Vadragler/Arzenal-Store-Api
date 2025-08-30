using ArzenalStoreApi.Data;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreApi.Models;
using ArzenalStoreSharedDto.DTOs.AppDto;
using Microsoft.EntityFrameworkCore;
// Ajoutez l'import du modèle App pour éviter la confusion avec le namespace



namespace ArzenalStoreApi.Services.AppService
{
    public class AppService(ApplicationDbContext context) : IAppService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<List<ReadAppDto>> GetAllAppsAsync()
        {
            var appsDto = await _context.Apps
    .Select(app => new ReadAppDto
    {
        Id = app.Id,
        Name = app.Name,
        Version = app.Version,
        FilePath = app.FilePath,
        Description = app.Description,
        IsVisible = app.IsVisible,
        IconePath = app.Icone,
        Requirements = app.Requirements,
        ReleaseDate = app.ReleaseDate,
        LastUpdated = app.LastUpdated,
        AppSize = app.AppSize,

        // Relations directes
        Category = app.Category!.Name,

        // Navigation properties (pas besoin de Join)
        Languages = app.AppLanguages!
            .Select(al => al.Language!.Name)
            .ToList()!,

        Tags = app.AppTags!
            .Select(at => at.Tag!.Name)
            .ToList()!,

        OperatingSystems = app.AppOperatingSystems!
            .Select(aos => aos.OperatingSystem!.Name)
            .ToList()!
    })
    .ToListAsync();

            if (appsDto.Count == 0)
                throw new NotFoundException("Aucune application trouvée.");

            return appsDto;

        }

        public async Task<ReadAppDto?> GetAppByIdAsync(Guid id)
        {
            var appDto = await _context.Apps
    .Where(a => a.Id == id)
    .Select(app => new ReadAppDto
    {
        Id = app.Id,
        Name = app.Name,
        Version = app.Version,
        FilePath = app.FilePath,
        Description = app.Description,
        IsVisible = app.IsVisible,
        IconePath = app.Icone,
        Requirements = app.Requirements,
        ReleaseDate = app.ReleaseDate,
        LastUpdated = app.LastUpdated,
        AppSize = app.AppSize,

        // Catégorie avec fallback si null
        Category = app.Category != null ? app.Category.Name : "Unknown",

        // Navigation properties directes (pas besoin de Join)
        Languages = app.AppLanguages!
            .Select(al => al.Language!.Name)
            .ToList()!,

        Tags = app.AppTags!
            .Select(at => at.Tag!.Name)
            .ToList()!,

        OperatingSystems = app.AppOperatingSystems!
            .Select(aos => aos.OperatingSystem!.Name)
            .ToList()!
    })
    .FirstOrDefaultAsync();

            if (appDto == null)
                throw new NotFoundException("Application introuvable");

            return appDto;
        }


        public async Task<ReadAppDto> CreateAppAsync(CreateAppDto Appdto)
        {
            // Vérifier si l'application existe déjà par son nom
            if (await _context.Apps.AnyAsync(a => a.Name == Appdto.Name))
            {
                throw new DuplicateException("Cette application existe déjà");
            }

            // Vérifie si la catégorie existe
            var category = await _context.Categories.FindAsync(Appdto.CategoryId);
            if (category == null)
            {
                throw new ValidationException("Catégorie intvalide");
            }

            // Vérifie si les langues sont valides
            if (Appdto.LanguageIds != null)
            {
                var invalidLanguages = Appdto.LanguageIds.Except(await _context.Languages.Select(l => l.Id).ToListAsync()).ToList();
                if (invalidLanguages.Count != 0)
                {
                    throw new ValidationException($"Les identifiants des langues suivantes sont invalides : {string.Join(", ", invalidLanguages)}");
                }
            }

            // Vérifie si les tags sont valides
            if (Appdto.TagIds != null)
            {
                var invalidTags = Appdto.TagIds.Except(await _context.Tags.Select(t => t.Id).ToListAsync()).ToList();
                if (invalidTags.Count != 0)
                {
                    throw new ValidationException($"Les identifiants des tags suivants sont invalides : {string.Join(", ", invalidTags)}");
                }
            }

            // Vérifie si les OS sont valides
            if (Appdto.OsIds != null)
            {
                var invalidOperatingSystems = Appdto.OsIds.Except(await _context.OperatingSystems.Select(os => os.Id).ToListAsync()).ToList();
                if (invalidOperatingSystems.Count != 0)
                {
                    throw new ValidationException($"Les identifiants des systèmes d'exploitation suivants sont invalides : {string.Join(", ", invalidOperatingSystems)}");
                }
            }

            // Créer une nouvelle entité App (et non CreateAppDto)
            var app = new App
            {
                Name = Appdto.Name,
                Version = Appdto.Version,
                FilePath = Appdto.FilePath,
                Description = Appdto.Description,
                IsVisible = Appdto.IsVisible,
                Icone = Appdto.IconePath,
                CategoryId = Appdto.CategoryId,
                LastUpdated = Appdto.LastUpdated,
                AppSize = Appdto.AppSize,
                Requirements = Appdto.Requirements,
                AppLanguages = Appdto.LanguageIds?.Select(id => new AppLanguage { LanguageId = id }).ToList(),
                AppTags = Appdto.TagIds?.Select(id => new AppTag { TagId = id }).ToList(),
                AppOperatingSystems = Appdto.OsIds?.Select(id => new AppOperatingSystem { OSId = id }).ToList()
            };


            _context.Apps.Add(app);
            await _context.SaveChangesAsync();

            // Retourner un ReadAppDto avec les infos de l'app créée
            var readAppDto = new ReadAppDto
            {
                Id = app.Id,
                Name = app.Name,
                Version = app.Version,
                FilePath = app.FilePath,
                Description = app.Description,
                IsVisible = app.IsVisible,
                IconePath = app.Icone,
                Requirements = app.Requirements,
                ReleaseDate = app.ReleaseDate,
                LastUpdated = app.LastUpdated,
                AppSize = app.AppSize,
                Category = category.Name,
                Languages = app.AppLanguages?.Select(al => _context.Languages.Find(al.LanguageId)?.Name ?? "Unknown").ToList() ?? [],
                Tags = app.AppTags?.Select(at => _context.Tags.Find(at.TagId)?.Name ?? "Unknown").ToList() ?? [],
                OperatingSystems = app.AppOperatingSystems?.Select(aos => _context.OperatingSystems.Find(aos.OSId)?.Name ?? "Unknown").ToList() ?? []
            };

            return readAppDto;
        
        }
        public async Task<UpdateAppDto?> UpdateAppAsync(Guid id, UpdateAppDto dto)
        {
            var app = await _context.Apps
                .Include(a => a.AppLanguages)
                .Include(a => a.AppTags)
                .Include(a => a.AppOperatingSystems)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null)
                throw new NotFoundException("Application introuvable");

            // ✅ Vérif du nom unique
            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != app.Name)
            {
                bool exists = await _context.Apps.AnyAsync(a => a.Name == dto.Name && a.Id != id);
                if (exists)
                    throw new DuplicateException("Une application avec ce nom existe déjà.");
                app.Name = dto.Name;
            }

            // ✅ Mise à jour simple (évite les if répétitifs)
            app.Version = dto.Version ?? app.Version;
            app.FilePath = dto.FilePath ?? app.FilePath;
            app.Description = dto.Description ?? app.Description;
            app.Icone = dto.IconePath ?? app.Icone;
            app.Requirements = dto.Requirements ?? app.Requirements;
            app.AppSize = dto.AppSize ?? app.AppSize;
            app.IsVisible = dto.IsVisible ?? app.IsVisible;

            app.LastUpdated = DateTime.UtcNow;

            // ✅ Catégorie
            if (dto.CategoryId.HasValue && dto.CategoryId.Value != app.CategoryId)
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value))
                    throw new ValidationException("Catégorie invalide");

                app.CategoryId = dto.CategoryId.Value;
            }

            // ✅ Mise à jour des collections (factorisé via méthode privée)
            if (dto.LanguageIds != null)
                app.AppLanguages = await ValidateAndReplaceCollection(
                    dto.LanguageIds,
                    _context.Languages.Select(l => l.Id),
                    invalid => $"Langues invalides : {string.Join(", ", invalid)}",
                    id => new AppLanguage { LanguageId = id, AppId = app.Id }
                );

            if (dto.TagIds != null)
                app.AppTags = await ValidateAndReplaceCollection(
                    dto.TagIds,
                    _context.Tags.Select(t => t.Id),
                    invalid => $"Tags invalides : {string.Join(", ", invalid)}",
                    id => new AppTag { TagId = id, AppId = app.Id }
                );

            if (dto.OsIds != null)
                app.AppOperatingSystems = await ValidateAndReplaceCollection(
                    dto.OsIds,
                    _context.OperatingSystems.Select(os => os.Id),
                    invalid => $"OS invalides : {string.Join(", ", invalid)}",
                    id => new AppOperatingSystem { OSId = id, AppId = app.Id }
                );

            await _context.SaveChangesAsync();
            return dto;
        }

        private async Task<List<T>> ValidateAndReplaceCollection<T, TKey>(
            IEnumerable<TKey> ids,
            IQueryable<TKey> validIdsQuery,
            Func<List<TKey>, string> errorMessage,
            Func<TKey, T> createEntity
        ) where T : class
        {
            var validIds = await validIdsQuery.ToListAsync();
            var invalidIds = ids.Except(validIds).ToList();

            if (invalidIds.Any())
                throw new ValidationException(errorMessage(invalidIds));

            return ids.Select(createEntity).ToList();
        }


        public async Task<bool> DeleteAppAsync(Guid id)
        {
            var app = await _context.Apps.FindAsync(id) ?? throw new NotFoundException("Application introuvable");
            _context.Apps.Remove(app);
           
                await _context.SaveChangesAsync();
                return true;
            
        }
    }
}
