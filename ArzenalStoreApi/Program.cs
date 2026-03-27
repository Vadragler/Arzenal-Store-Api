using Arzenal.Dto;
using Arzenal.Store.Api.Infrastructure.Data;
using ArzenalStoreApi.DependencyInjection;
using ArzenalStoreApi.Middleware;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

/*builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Arzenal API",
        Version = "v1",
        Description = "API pour gérer Arzenal Store"
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    };
    var securityRequirement = new OpenApiSecurityRequirement();
    securityRequirement.Add(securityScheme, new List<string>());

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityRequirement);
});*/


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

builder.Services.AddJwtAuthentication(builder.Configuration);

// Configuration des journaux
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Ajouter les services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("StoreConnection"),
        new MySqlServerVersion(new Version(8, 0, 33)));
});
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("AuthConnection"),
        new MySqlServerVersion(new Version(8, 0, 33)));
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024L * 1024 * 1024; // 1 Go
});


builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection("Storage"));

builder.Services
    .AddApplicationServices()
    .AddAuthServices()
    .AddMappers()
    .AddRequestProviders();

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = null; // conserve la casse exacte
}); 
// Ajoute les validators manuellement ou automatiquement
builder.Services.AddValidatorsFromAssemblyContaining<ValidatorAssemblyReference>();

// Active la validation automatique via les filtres d'API
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
     var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    if (env.IsEnvironment("Testing"))
    {
        try
        {
            var storeDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            storeDb.Database.Migrate();

            var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            authDb.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'application des migrations : {ex.Message}");
            throw;
        }
    }
}

if (app.Environment.IsDevelopment()|| app.Environment.IsEnvironment("Testing"))
{
    // Enable Swagger only if services are registered to avoid runtime resolution errors in tests when AddSwaggerGen is not called.
    var swaggerProvider = app.Services.GetService(typeof(Swashbuckle.AspNetCore.Swagger.ISwaggerProvider));
    if (swaggerProvider != null)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}

app.UseCors("AllowAngularApp");

app.UseMiddleware<DatabaseExceptionMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
//app.UseMiddleware<JwtCookieToHeaderMiddleware>();
app.UseMiddleware<GuidValidationMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }