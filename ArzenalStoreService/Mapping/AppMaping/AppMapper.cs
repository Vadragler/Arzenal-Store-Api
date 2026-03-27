using Arzenal.Store.Api.Domain.Models;
using Arzenal.Dto.DTOs.AppDto;
using Riok.Mapperly.Abstractions;
using System.Linq;

namespace Arzenal.Store.Api.Service.Mapping.AppMaping
{
    [Mapper]
    public partial class AppMapper
    {
        // App -> ReadAppDto
        public ReadAppDto ToReadAppDto(App app)
        {
            if (app == null) return null;

            var target = new ReadAppDto
            {
                Id = app.Id,
                Name = app.Name,
                Version = app.Version,
                Description = app.Description,
                IsVisible = app.IsVisible,
                IconePath = app.Icone,
                Requirements = app.Requirements,
                ReleaseDate = app.ReleaseDate,
                LastUpdated = app.LastUpdated,
                AppSize = app.AppSize,
                Category = app.Category?.Name,
                Languages = app.AppLanguages?
                  .Where(al => al?.Language != null)
                  .Select(al => al.Language.Name)
                  .ToList(),

                Tags = app.AppTags?
                        .Where(at => at?.Tag != null)
                        .Select(at => at.Tag.Name)
                        .ToList(),

                OperatingSystems = app.AppOperatingSystems?
                     .Where(os => os?.OperatingSystem != null)
                     .Select(os => os.OperatingSystem.Name)
                     .ToList()

            };

            return target;
        }

        // CreateAppDto -> App
        public App ToApp(CreateAppDto dto)
        {
            if (dto == null) return null;

            var target = new App
            {
                Name = dto.Name,
                Version = dto.Version,
                Description = dto.Description,
                IsVisible = dto.IsVisible,
                CategoryId = dto.CategoryId,
                LastUpdated = dto.LastUpdated,
                AppSize = dto.AppSize ?? 0,
                Requirements = dto.Requirements
            };

            return target;
        }

        // UpdateAppDto -> App (update existant)
        public void UpdateAppFromDto(UpdateAppDto dto, App app)
        {
            if (dto == null || app == null) return;

            if (dto.Name != null) app.Name = dto.Name;
            if (dto.Version != null) app.Version = dto.Version;
            app.Description = dto.Description;
            if (dto.IsVisible.HasValue) app.IsVisible = dto.IsVisible.Value;
            app.CategoryId = dto.CategoryId;
            if (dto.LastUpdated.HasValue) app.LastUpdated = dto.LastUpdated.Value;
            if (dto.AppSize.HasValue) app.AppSize = dto.AppSize.Value;
            app.Requirements = dto.Requirements;
        }
    }
}
