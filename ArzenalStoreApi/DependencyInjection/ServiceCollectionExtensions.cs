using Arzenal.Store.Api.Service.Services.Auth.Passwords;
using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Mapping.AppMaping;
using Arzenal.Store.Api.Service.Mapping.CategoryMapping;
using Arzenal.Store.Api.Service.Mapping.LanguageMapping;
using Arzenal.Store.Api.Service.Mapping.OperatingSystemMapping;
using Arzenal.Store.Api.Service.Mapping.TagMapping;
using Arzenal.Store.Api.Service.Services.Accounts;
using Arzenal.Store.Api.Service.Services.AppFilesService;
using Arzenal.Store.Api.Service.Services.AppService;
using Arzenal.Store.Api.Service.Services.Auth;
using Arzenal.Store.Api.Service.Services.Auth.Cookies;
using Arzenal.Store.Api.Service.Services.Auth.Invites;
using Arzenal.Store.Api.Service.Services.Auth.RequestContext;
using Arzenal.Store.Api.Service.Services.Auth.RequestInfo;
using Arzenal.Store.Api.Service.Services.Auth.Tokens;
using Arzenal.Store.Api.Service.Services.CategoryService;
using Arzenal.Store.Api.Service.Services.LanguageService;
using Arzenal.Store.Api.Service.Services.OperatingSystemService;
using Arzenal.Store.Api.Service.Services.TagService;

namespace ArzenalStoreApi.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAppService, AppService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IOperatingSystemService, OperatingSystemService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IAppFilesService, AppFilesService>();

        return services;
    }

    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInviteService, InviteService>();
        services.AddScoped<IJwtCookieService, JwtCookieService>();
        services.AddScoped<IDeviceService, DeviceService>();

        return services;
    }

    public static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddScoped<AppMapper>();
        services.AddScoped<CategoryMapper>();
        services.AddScoped<LanguageMapper>();
        services.AddScoped<OperatingSystemMapper>();
        services.AddScoped<TagMapper>();

        services.AddScoped<IAppMapper, AppMapperWrapper>();
        services.AddScoped<ICategoryMapper, CategoryMapperWrapper>();
        services.AddScoped<ILanguageMapper, LanguageMapperWrapper>();
        services.AddScoped<IOperatingSystemMapper, OperatingSystemMapperWrapper>();
        services.AddScoped<ITagMapper, TagMapperWrapper>();

        return services;
    }

    public static IServiceCollection AddRequestProviders(this IServiceCollection services)
    {
        services.AddScoped<IRequestContextProvider, RequestContextProvider>();
        services.AddScoped<IRequestInfoProvider, RequestInfoProvider>();

        return services;
    }
}