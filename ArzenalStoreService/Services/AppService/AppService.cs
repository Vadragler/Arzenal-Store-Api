using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Domain.Models;
using ArzenalStoreInfrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Mapping.AppMaping;
using Arzenal.Dto.DTOs.AppDto;
using Microsoft.EntityFrameworkCore;


namespace Arzenal.Store.Api.Service.Services.AppService
{
    public class AppService: IAppService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppMapper _mapper;

        public AppService(ApplicationDbContext context,IAppMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<ReadAppDto>> GetAllAppsAsync()
        {
            var apps = await _context.Apps
                .Include(a => a.Category)
                .Include(a => a.AppLanguages)
                    !.ThenInclude(al => al.Language)
                .Include(a => a.AppTags)
                    !.ThenInclude(at => at.Tag)
                .Include(a => a.AppOperatingSystems)
                    !.ThenInclude(aos => aos.OperatingSystem)
                .ToListAsync();

            return apps.Select(a => _mapper.ToReadAppDto(SafeApp(a))).ToList();
        }

        #region Private Methods GetAllAppsAsync
        private App SafeApp(App app)
        {
            app.AppLanguages ??= new List<AppLanguage>();
            app.AppTags ??= new List<AppTag>();
            app.AppOperatingSystems ??= new List<AppOperatingSystem>();
            return app;
        }
        #endregion

        public async Task<ReadAppDto> GetAppByIdAsync(Guid id)
        {
            var app = await _context.Apps
                .Include(a => a.Category)
                .Include(a => a.AppLanguages)!.ThenInclude(al => al.Language)
                .Include(a => a.AppTags)!.ThenInclude(at => at.Tag)
                .Include(a => a.AppOperatingSystems)!.ThenInclude(aos => aos.OperatingSystem)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null)
                throw new NotFoundException("Application introuvable");

            return _mapper.ToReadAppDto(app);
        }

        
        public async Task<ReadAppDto> CreateAppAsync(CreateAppDto Appdto)
        {
            if (await _context.Apps.AnyAsync(a => a.Name == Appdto.Name))
                throw new DuplicateException("Cette application existe déjà");
            

            await ValidateCategoryAsync(Appdto.CategoryId);
            await ValidateIdsAsync(Appdto.LanguageIds, _context.Languages.Select(l => l.Id), "Langues invalides");
            await ValidateIdsAsync(Appdto.TagIds, _context.Tags.Select(t => t.Id), "Tags invalides");
            await ValidateIdsAsync(Appdto.OsIds, _context.OperatingSystems.Select(os => os.Id), "OS invalides");

            // Créer l'entité
            var app = _mapper.ToApp(Appdto);

            _context.Apps.Add(app);
            await _context.SaveChangesAsync();

            // Charger les références pour le ReadAppDto
            await LoadAppRelationsAsync(app);   

            return _mapper.ToReadAppDto(app);

        }

        #region Private Methods CreateAppAsync
        private async Task LoadAppRelationsAsync(App app)
        {
            await _context.Entry(app).Reference(a => a.Category).LoadAsync();
            await _context.Entry(app).Collection(a => a.AppLanguages!).Query().Include(x => x.Language).LoadAsync();
            await _context.Entry(app).Collection(a => a.AppTags!).Query().Include(x => x.Tag).LoadAsync();
            await _context.Entry(app).Collection(a => a.AppOperatingSystems!).Query().Include(x => x.OperatingSystem).LoadAsync();
        }

        private async Task ValidateCategoryAsync(Guid? categoryId)
        {
            if (await _context.Categories.FindAsync(categoryId) == null)
                throw new ValidationException("Catégorie invalide");
        }

        private async Task ValidateIdsAsync(IEnumerable<Guid>? ids, IQueryable<Guid> source, string error)
        {
            if (ids == null) return;

            var validIds = await source.ToListAsync();
            var invalid = ids.Except(validIds).ToList();

            if (invalid.Count != 0)
                throw new ValidationException($"{error} : {string.Join(", ", invalid)}");
        }
        #endregion

        public async Task<ReadAppDto> UpdateAppAsync(Guid id, UpdateAppDto dto)
        {
            var app = await _context.Apps
                .Include(a => a.AppLanguages)
                .Include(a => a.AppTags)
                .Include(a => a.AppOperatingSystems)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null)
                throw new NotFoundException("Application introuvable");

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != app.Name)
            {
                bool exists = await _context.Apps.AnyAsync(a => a.Name == dto.Name && a.Id != id);
                if (exists)
                    throw new DuplicateException("Une application avec ce nom existe déjà.");
                app.Name = dto.Name;
            }

            app.Version = dto.Version ?? app.Version;
            app.Description = dto.Description ?? app.Description;
            app.Icone = dto.IconePath ?? app.Icone;
            app.Requirements = dto.Requirements ?? app.Requirements;
            app.AppSize = dto.AppSize ?? app.AppSize;
            app.IsVisible = dto.IsVisible ?? app.IsVisible;
            app.LastUpdated = DateTime.UtcNow;

            if (dto.CategoryId.HasValue && dto.CategoryId.Value != app.CategoryId)
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value))
                    throw new ValidationException("Catégorie invalide");

                app.CategoryId = dto.CategoryId.Value;
            }

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
            return _mapper.ToReadAppDto(app);
        }
        #region Private Methods UpdateAppAsync
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
        #endregion

        public async Task<bool> DeleteAppAsync(Guid id)
        {
            var app = await _context.Apps.FindAsync(id) ?? throw new NotFoundException("Application introuvable");
            _context.Apps.Remove(app);
           
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
