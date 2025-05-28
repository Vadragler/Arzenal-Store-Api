using Microsoft.AspNetCore.Mvc;
using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Arzenal.Shared.Dtos.DTOs.AppDto;


namespace ArzenalStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<App>>> GetAllApps()
        {
            // On récupère les apps avec leurs relations (Languages, Tags, OS, Category)
            var appDtos = await _context.Apps
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

                    // Catégorie (avec le nom de la catégorie)
                    Category = app.Category!.Name,

                    // Listes des langues, tags et systèmes d'exploitation
                    Languages = app.AppLanguages!
                        .Select(al => al.LanguageId)  // On récupère l'ID de la langue
                        .Join(_context.Languages,  // On joint avec la table Language pour récupérer le nom
                              languageId => languageId,  // ID de la langue dans AppLanguage
                              language => language.Id,  // ID de la langue dans Language
                              (languageId, language) => language.Name)  // On sélectionne le nom de la langue
                        .ToList()!,

                    Tags = app.AppTags!
                        .Select(at => at.TagId)  // On récupère l'ID du tag
                        .Join(_context.Tags,  // On joint avec la table Tag pour récupérer le nom
                              tagId => tagId,  // ID du tag dans AppTag
                              tag => tag.Id,  // ID du tag dans Tag
                              (tagId, tag) => tag.Name)  // On sélectionne le nom du tag
                        .ToList()!,

                    OperatingSystems = app.AppOperatingSystems!
                        .Select(aos => aos.OSId)  // On récupère l'ID de l'OS
                        .Join(_context.OperatingSystems,  // On joint avec la table OperatingSystem pour récupérer le nom
                              osId => osId,  // ID de l'OS dans AppOperatingSystem
                              os => os.Id,  // ID de l'OS dans OperatingSystem
                              (osId, os) => os.Name)  // On sélectionne le nom de l'OS
                        .ToList()!
                })
                .ToListAsync();


            // Transformation des données d'app en AppDto


            // Retourner les données sous forme de JSON
            return Ok(appDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReadAppDto>> GetAppById(Guid id)
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

                    // Catégorie (avec protection contre les nulls)
                    Category = app.Category!.Name ?? "Unknown",  // Si Category est null, "Unknown"

                    // Listes des langues, tags et systèmes d'exploitation
                    Languages = app.AppLanguages!
                        .Select(al => al.LanguageId)
                        .Join(_context.Languages,
                            languageId => languageId,
                            language => language.Id,
                            (languageId, language) => language.Name)
                        .ToList()!,

                    Tags = app.AppTags!
                        .Select(at => at.TagId)
                        .Join(_context.Tags,
                            tagId => tagId,
                            tag => tag.Id,
                            (tagId, tag) => tag.Name)
                        .ToList()!,

                    OperatingSystems = app.AppOperatingSystems!
                        .Select(aos => aos.OSId)
                        .Join(_context.OperatingSystems,
                            osId => osId,
                            os => os.Id,
                            (osId, os) => os.Name)
                        .ToList()!
                })
                .FirstOrDefaultAsync();

            if (appDto == null)
            {
                return NotFound();  // Si l'application n'est pas trouvée, retourne 404.
            }

            return Ok(appDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateApp([FromBody] CreateAppDto Appdto)
        {
            // Vérifier si l'application existe déjà par son nom
            if (await _context.Apps.AnyAsync(a => a.Name == Appdto.Name))
            {
                return BadRequest("Cette application existe déjà");
            }

            // Vérifie si la catégorie existe
            var category = await _context.Categories.FindAsync(Appdto.CategoryId);
            if (category == null)
            {
                return BadRequest("Catégorie invalide");
            }

            // Vérifie si les langues sont valides
            if (Appdto.LanguageIds != null)
            {
                var invalidLanguages = Appdto.LanguageIds.Except(await _context.Languages.Select(l => l.Id).ToListAsync()).ToList();
                if (invalidLanguages.Count != 0)
                {
                    return BadRequest($"Les identifiants des langues suivantes sont invalides : {string.Join(", ", invalidLanguages)}");
                }
            }

            // Vérifie si les tags sont valides
            if (Appdto.TagIds != null)
            {
                var invalidTags = Appdto.TagIds.Except(await _context.Tags.Select(t => t.Id).ToListAsync()).ToList();
                if (invalidTags.Count != 0)
                {
                    return BadRequest($"Les identifiants des tags suivants sont invalides : {string.Join(", ", invalidTags)}");
                }
            }

            // Vérifie si les OS sont valides
            if (Appdto.OsIds != null)
            {
                var invalidOperatingSystems = Appdto.OsIds.Except(await _context.OperatingSystems.Select(os => os.Id).ToListAsync()).ToList();
                if (invalidOperatingSystems.Count != 0)
                {
                    return BadRequest($"Les identifiants des systèmes d'exploitation suivants sont invalides : {string.Join(", ", invalidOperatingSystems)}");
                }
            }

            // Créer une nouvelle application
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
            };

            // Liens avec Languages
            if (Appdto.LanguageIds != null)
            {
                app.AppLanguages = [.. Appdto.LanguageIds.Select(id => new AppLanguage
                {
                    LanguageId = id
                })];
            }

            // Liens avec Tags
            if (Appdto.TagIds != null)
            {
                app.AppTags = [.. Appdto.TagIds.Select(id => new AppTag
                {
                    TagId = id
                })];
            }

            // Liens avec OS
            if (Appdto.OsIds != null)
            {
                app.AppOperatingSystems = [.. Appdto.OsIds.Select(id => new AppOperatingSystem
                {
                    OSId = id
                })];
            }

            try
            {
                // Ajouter l'application à la base de données
                _context.Apps.Add(app);
                await _context.SaveChangesAsync();

                // Retourner la réponse avec l'application créée (id et autres détails)
                return CreatedAtAction(nameof(GetAppById), new { id = app.Id }, new { app.Id });
            }
            catch (Exception ex)
            {
                // Gérer les erreurs internes
                return StatusCode(500, $"Erreur interne : {ex.Message}");
            }
        }



        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateApp(Guid id, [FromBody] UpdateAppDto updateAppDto)
        {
            // Vérifier si l'application existe
            var app = await _context.Apps
                .Include(a => a.AppLanguages)
                .Include(a => a.AppTags)
                .Include(a => a.AppOperatingSystems)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null)
            {
                return NotFound(); // Si l'App n'existe pas, retourne une erreur 404
            }

            if (await _context.Apps.AnyAsync(a => a.Name == updateAppDto.Name && a.Id!=id))
            {
                return BadRequest("Cette application existe déjà");
            }

            // Vérifier l'existence de la catégorie si spécifiée
            if (updateAppDto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateAppDto.CategoryId.Value);
                if (!categoryExists)
                {
                    return BadRequest("Catégorie invalide");
                }
            }

            // Vérification de l'unicité du nom si modifié
            if (!string.IsNullOrWhiteSpace(updateAppDto.Name))
            {
                var nameExists = await _context.Apps
                    .AnyAsync(a => a.Name == updateAppDto.Name && a.Id != id);

                if (nameExists)
                {
                    return BadRequest("Cette application existe déjà");
                }

                app.Name = updateAppDto.Name;
            }

            // Mise à jour des champs simples
            if (!string.IsNullOrWhiteSpace(updateAppDto.Version))
                app.Version = updateAppDto.Version;

            if (!string.IsNullOrWhiteSpace(updateAppDto.FilePath))
                app.FilePath = updateAppDto.FilePath;

            if (!string.IsNullOrWhiteSpace(updateAppDto.Description))
                app.Description = updateAppDto.Description;

            if (updateAppDto.IsVisible.HasValue)
                app.IsVisible = updateAppDto.IsVisible.Value;

            if (!string.IsNullOrWhiteSpace(updateAppDto.IconePath))
                app.Icone = updateAppDto.IconePath;

            if (updateAppDto.CategoryId.HasValue)
                app.CategoryId = updateAppDto.CategoryId.Value;

            if (updateAppDto.LastUpdated.HasValue)
                app.LastUpdated = updateAppDto.LastUpdated.Value;

            if (updateAppDto.AppSize.HasValue)
                app.AppSize = updateAppDto.AppSize.Value;

            if (!string.IsNullOrWhiteSpace(updateAppDto.Requirements))
                app.Requirements = updateAppDto.Requirements;

            // 🔁 Mise à jour des langues
            if (updateAppDto.LanguageIds is not null)
            {
                var validLanguageIds = await _context.Languages.Select(l => l.Id).ToListAsync();
                var invalidLanguages = updateAppDto.LanguageIds.Except(validLanguageIds).ToList();

                if (invalidLanguages.Any())
                {
                    return BadRequest($"Les identifiants des langues suivants sont invalides : {string.Join(", ", invalidLanguages)}");
                }

                _context.AppLanguages.RemoveRange(app.AppLanguages!);
                app.AppLanguages = updateAppDto.LanguageIds
                    .Select(id => new AppLanguage { AppId = app.Id, LanguageId = id })
                    .ToList();
            }

            // 🔁 Mise à jour des tags
            if (updateAppDto.TagIds is not null)
            {
                var validTagIds = await _context.Tags.Select(t => t.Id).ToListAsync();
                var invalidTags = updateAppDto.TagIds.Except(validTagIds).ToList();

                if (invalidTags.Any())
                {
                    return BadRequest($"Les identifiants des tags suivants sont invalides : {string.Join(", ", invalidTags)}");
                }

                _context.AppTags.RemoveRange(app.AppTags!);
                app.AppTags = updateAppDto.TagIds
                    .Select(id => new AppTag { AppId = app.Id, TagId = id })
                    .ToList();
            }

            // 🔁 Mise à jour des systèmes d'exploitation
            if (updateAppDto.OsIds is not null)
            {
                var validOsIds = await _context.OperatingSystems.Select(os => os.Id).ToListAsync();
                var invalidOs = updateAppDto.OsIds.Except(validOsIds).ToList();

                if (invalidOs.Any())
                {
                    return BadRequest($"Les identifiants des systèmes d'exploitation suivants sont invalides : {string.Join(", ", invalidOs)}");
                }

                _context.AppOperatingSystems.RemoveRange(app.AppOperatingSystems!);
                app.AppOperatingSystems = updateAppDto.OsIds
                    .Select(id => new AppOperatingSystem { AppId = app.Id, OSId = id })
                    .ToList();
            }

            // Sauvegarder les modifications
            await _context.SaveChangesAsync();

            // Préparer la réponse
            var updatedAppDto = new ReadAppDto
            {
                Id = app.Id,
                Name = app.Name,
                Version = app.Version,
                FilePath = app.FilePath,
                Description = app.Description,
                IsVisible = app.IsVisible,
                IconePath = app.Icone,
                Category = await _context.Categories
                    .Where(c => c.Id == app.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync(),
                ReleaseDate = app.ReleaseDate,
                LastUpdated = app.LastUpdated,
                AppSize = app.AppSize,
                Requirements = app.Requirements,
                Languages = await _context.Languages!
                    .Where(l => app.AppLanguages.Select(al => al.LanguageId).Contains(l.Id))
                    .Select(l => l.Name)
                    .ToListAsync()!,
                Tags = await _context.Tags!
                    .Where(t => app.AppTags.Select(at => at.TagId).Contains(t.Id))
                    .Select(t => t.Name)
                    .ToListAsync()!,
                OperatingSystems = await _context.OperatingSystems!
                    .Where(os => app.AppOperatingSystems.Select(aos => aos.OSId).Contains(os.Id))
                    .Select(os => os.Name)
                    .ToListAsync()!
            };

            return Ok(updatedAppDto);
        }






        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApp(Guid id)
        {
            // Recherche de l'application avec ses relations
            var app = await _context.Apps
                 .Include(a => a.AppLanguages)
                .Include(a => a.AppTags)
                .Include(a => a.AppOperatingSystems)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null)
                return NotFound(); // Si l'application n'est pas trouvée, retourne 404

            try
            {
                // Supprimer les relations si elles existent
                if (app.AppLanguages != null)
                    _context.AppLanguages.RemoveRange(app.AppLanguages);
                if (app.AppTags != null)
                    _context.AppTags.RemoveRange(app.AppTags);
                if (app.AppOperatingSystems != null)
                    _context.AppOperatingSystems.RemoveRange(app.AppOperatingSystems);

                // Supprimer l'application elle-même
                _context.Apps.Remove(app);

                // Sauvegarder les changements dans la base de données
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // En cas d'erreur pendant la suppression
                return StatusCode(500, $"Internal error: {ex.Message}");
            }

            return NoContent(); // Retourne 204 en cas de succès
        }

    }
}
