using Arzenal.Store.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.InviteControllerTests
{
    public class InviteControllerSuccessTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public InviteControllerSuccessTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateAuthenticatedClient();
        }

        // -----------------------------
        // ValidateToken (GET /invite/validate)
        // -----------------------------
        [Fact]
        public async Task ValidateToken_ReturnsOk_WhenTokenIsValid()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            var token = "valid-token";
            db.InviteTokens.Add(new InviteToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                Email = "test@test.com",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                Used = false
            });
            await db.SaveChangesAsync();

            var response = await _client.GetAsync($"api/invite/validate?token={token}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // -----------------------------
        // GenerateToken (POST /invite/generate)
        // -----------------------------
        [Fact]
        public async Task GenerateToken_ReturnsOk_AndCreatesInvite()
        {
            var email = "newuser@example.com";
            var response = await _client.PostAsJsonAsync("api/invite/generate", email);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

            Assert.NotNull(json);
            Assert.True(json.ContainsKey("Link"));
            Assert.Contains("signup?token=", json["Link"]);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            Assert.True(db.InviteTokens.Any(i => i.Email == email));
        }

        // -----------------------------
        // UseToken (POST /invite/use)
        // -----------------------------
        [Fact]
        public async Task UseToken_ReturnsOk_WhenTokenIsValid()
        {
            // Étape 1 : générer le token via le controller
            var email = "user@mail.com";
            var generateResponse = await _client.PostAsJsonAsync("api/invite/generate", email);
            generateResponse.EnsureSuccessStatusCode();

            var json = await generateResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var link = json!["Link"];

            // Étape 2 : extraire le token du lien
            var tokenString = link.Split("token=").Last();

            // Étape 3 : appeler UseToken avec ce token
            var response = await _client.PostAsync($"api/invite/use?token={tokenString}", null);
            response.EnsureSuccessStatusCode();

            // Étape 4 : vérifier dans la DB
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            var invite = await db.InviteTokens.FirstOrDefaultAsync(i => i.Email == email);
            Assert.NotNull(invite);
            Assert.True(invite!.Used);
        }

    }
}
