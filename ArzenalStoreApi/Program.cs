using Arzenal.Dto;
using Arzenal.Store.Api.Infrastructure.Data;
using ArzenalStoreApi.DependencyInjection;
using ArzenalStoreApi.Health;
using ArzenalStoreApi.Middleware;
using ArzenalStoreInfrastructure.Configurations;
using ArzenalStoreInfrastructure.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.RateLimiting;

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

builder.Services.AddSwaggerGen();


builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    var knownProxies = builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [];
    foreach (var proxy in knownProxies)
    {
        if (IPAddress.TryParse(proxy, out var address))
            options.KnownProxies.Add(address);
    }
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext => CreateSlidingWindowPolicy(httpContext, 8, TimeSpan.FromMinutes(15)));
    options.AddPolicy("refresh", httpContext => CreateSlidingWindowPolicy(httpContext, 30, TimeSpan.FromMinutes(15)));
    options.AddPolicy("invite", httpContext => CreateSlidingWindowPolicy(httpContext, 12, TimeSpan.FromMinutes(15)));
    options.AddPolicy("app-login", httpContext => CreateSlidingWindowPolicy(httpContext, 20, TimeSpan.FromMinutes(15)));
    options.AddPolicy("upload", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name
                ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 12,
                Window = TimeSpan.FromHours(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.OnRejected = (context, _) =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");
        logger.LogWarning("Rate limit exceeded for {Path} from {Address}",
            context.HttpContext.Request.Path,
            context.HttpContext.Connection.RemoteIpAddress);
        return ValueTask.CompletedTask;
    };
});

static RateLimitPartition<string> CreateSlidingWindowPolicy(HttpContext httpContext, int permitLimit, TimeSpan window) =>
    RateLimitPartition.GetSlidingWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            SegmentsPerWindow = 3,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        });

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
builder.Services.AddHealthChecks()
    .AddCheck<DbContextHealthCheck<ApplicationDbContext>>("store-database", tags: ["ready"])
    .AddCheck<DbContextHealthCheck<AuthDbContext>>("auth-database", tags: ["ready"]);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 256L * 1024 * 1024;
});


builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<MalwareScanSettings>(builder.Configuration.GetSection("MalwareScan"));

if (builder.Environment.IsProduction() &&
    (!builder.Configuration.GetValue<bool>("Storage:RequireMalwareScan") ||
     !builder.Configuration.GetValue<bool>("MalwareScan:Enabled")))
{
    throw new InvalidOperationException("Le scan ClamAV doit être activé et requis en production.");
}

builder.Services
    .AddApplicationServices()
    .AddAuthServices()
    .AddMappers()
    .AddRequestProviders()
    .AddMemoryCache();

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
    var applyMigrationsOnStartup = env.IsEnvironment("Testing") ||
        builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
    if (applyMigrationsOnStartup)
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

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    var swaggerProvider = app.Services.GetService(typeof(Swashbuckle.AspNetCore.Swagger.ISwaggerProvider));
    if (swaggerProvider != null)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}

app.UseForwardedHeaders();
app.UseCors("AllowAngularApp");
app.UseMiddleware<ExceptionHandlingMiddleware>();
//app.UseMiddleware<JwtCookieToHeaderMiddleware>();
app.UseMiddleware<GuidValidationMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") }).AllowAnonymous();

app.Run();
public partial class Program { }
