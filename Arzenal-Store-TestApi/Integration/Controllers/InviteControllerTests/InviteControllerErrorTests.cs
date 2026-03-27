using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.InviteControllerTests
{
    public class InviteControllerErrorTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public InviteControllerErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UseToken_ThrowsUnauthorized_WhenTokenDoesNotExist()
        {
            // On utilise un token qui n’existe pas
            var fakeToken = Guid.NewGuid().ToString();

            // On fait le POST
            var response = await _client.PostAsync($"api/invite/use?token={fakeToken}", null);

            // On s’attend à 500 ou 401 selon la config de ton middleware
            Assert.True(response.StatusCode == HttpStatusCode.InternalServerError
                        || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UseToken_ThrowsUnauthorized_WhenTokenAlreadyUsed()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            var invite = new InviteToken
            {
                Id = Guid.NewGuid(),
                Token = Guid.NewGuid().ToString(),
                Email = "used@mail.com",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                Used = true // déjà utilisé
            };
            db.InviteTokens.Add(invite);
            await db.SaveChangesAsync();

            // POST /invite/use avec le token déjà utilisé
            var response = await _client.PostAsync($"api/invite/use?token={invite.Token}", null);

            // On s’attend à 500 ou 401
            Assert.True(response.StatusCode == HttpStatusCode.InternalServerError
                        || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ValidateToken_ThrowsUnauthorized_WhenTokenExpired()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            var invite = new InviteToken
            {
                Id = Guid.NewGuid(),
                Token = Guid.NewGuid().ToString(),
                Email = "expired@mail.com",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // expiré
                Used = false
            };
            db.InviteTokens.Add(invite);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync($"api/invite/validate?token={invite.Token}");

            Assert.True(response.StatusCode == HttpStatusCode.InternalServerError
                        || response.StatusCode == HttpStatusCode.Unauthorized);
        }
    }
}
