using ArzenalApi.Data;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ArzenalApi.Services;
using ArzenalApi.Middleware;
using FluentValidation;
using Arzenal.Shared.Dtos;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:56525") // Remplace par l'URL de ton app Angular
              .AllowAnyHeader()
              .AllowAnyMethod();
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

app.UseHttpsRedirection();


app.UseAuthentication(); // Active l'authentification
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }