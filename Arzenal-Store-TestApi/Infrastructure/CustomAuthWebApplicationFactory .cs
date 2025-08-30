using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestArzenalStoreApi.Infrastructure
{
    public class CustomAuthWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private SqliteConnection _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Supprime l'ancien DbContext s’il existe
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

               
                    // Supprimer l’authentification existante (JWT réel par ex.)
                    services.AddAuthentication("Fake")
                        .AddScheme<AuthenticationSchemeOptions, FakeJwtAuthHandler>("Fake", options => { });
               
              
                // Nouvelle connexion SQLite en mémoire
                _connection = new SqliteConnection("Filename=:memory:");
                _connection.Open();

                // Ajoute le contexte avec cette connexion
                services.AddDbContext<AuthDbContext>(options =>
                    options.UseSqlite(_connection));

                // Crée la base
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                db.Database.EnsureCreated();

                // Token valide pour tests
                db.InviteTokens.Add(new InviteToken
                {
                    Email = "test@exemple.com",
                    Token = "valid-token",
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    Used = false
                });

                // Utilisateur existant pour déclencher DuplicateException
                db.Users.Add(new User
                {
                    Username = "existinguser",
                    Email = "existing@example.com",
                    PasswordHash = "hashed-password"
                });
                db.SaveChanges();
                
                // Optionnel : ajouter ici des utilisateurs de test (Seed)
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
    }

}
