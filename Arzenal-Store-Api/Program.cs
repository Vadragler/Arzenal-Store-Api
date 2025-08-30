using ArzenalStoreApi.Data;
using ArzenalStoreApi.Middleware;
using ArzenalStoreApi.Services.AppService;
using ArzenalStoreApi.Services.Auth;
using ArzenalStoreApi.Services.CategoryService;
using ArzenalStoreApi.Services.CookieService;
using ArzenalStoreApi.Services.LanguageService;
using ArzenalStoreApi.Services.OperatingSystemService;
using ArzenalStoreApi.Services.PasswordService;
using ArzenalStoreApi.Services.RequestContextProvider;
using ArzenalStoreApi.Services.RequestInfoProvider;
using ArzenalStoreApi.Services.TagService;
using ArzenalStoreApi.Services.Token;
using ArzenalStoreApi.Services.UserService;
using ArzenalStoreSharedDto;
using ArzenalStoreSharedDto.Validators;
using ArzenalStoreSharedDto.Validators.AppDtoValidators;
using ArzenalStoreSharedDto.Validators.AuthDtoValidators;
using ArzenalStoreSharedDto.Validators.CategorieDtoValidators;
using ArzenalStoreSharedDto.Validators.LanguageDtoValidators;
using ArzenalStoreSharedDto.Validators.OperatingSystemDtoValidators;
using ArzenalStoreSharedDto.Validators.TagDtoValidators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:56525") // Remplace par l'URL de ton app Angular
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddScoped<IAuthService, AuthService>();

// Configuration des journaux
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Ajouter les services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                     ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));
});
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("AuthConnection"),
                     ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("AuthConnection")));
});

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAppService, AppService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<IOperatingSystemService, OperatingSystemService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IRequestContextProvider, RequestContextProvider>();
builder.Services.AddScoped<IRequestInfoProvider, RequestInfoProvider>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUserService, UserService>();



// Configuration de l'authentification JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],  // Ajoute ton issuer dans les settings
            ValidAudience = builder.Configuration["Jwt:Audience"], // Ajoute ton audience dans les settings
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])) // Clé secrète de signature
        };
    });


// Add services to the container.

builder.Services.AddControllers();
// Ajoute les validators manuellement ou automatiquement
builder.Services.AddValidatorsFromAssemblyContaining<ValidatorAssemblyReference>();


// Active la validation automatique via les filtres d'API
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowAngularApp"); // Utilise la politique CORS définie
app.UseMiddleware<DatabaseExceptionMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();


app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["authToken"];
    if (!string.IsNullOrEmpty(token) && !context.Request.Headers.ContainsKey("Authorization"))
    {
        context.Request.Headers.Add("Authorization", $"Bearer {token}");
    }
    await next();
});

app.UseAuthentication(); // Active l'authentification
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }