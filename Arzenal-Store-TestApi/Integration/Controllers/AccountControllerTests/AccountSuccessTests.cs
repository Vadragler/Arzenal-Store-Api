using Arzenal.Dto.DTOs.AccountDto;
using Arzenal.Dto.DTOs.AuthDto;
using Arzenal.Store.Api.Service.Services.Auth.Passwords;
using Arzenal.Store.Api.TestIntegration.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.AccountControllerTests
{
    public class AccountSuccessTests 
    {

        [Fact]
        public async Task GetCurrentUser_ReturnsCurrentUser()
        {
            await using var factory = new CustomWebApplicationFactory<Program>();
            var client = factory.CreateAuthenticatedClient("Chrome");
            var response = await client.GetAsync("api/account/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<ReadAccountDto>();
            Assert.NotNull(dto);
            Assert.Equal("existinguser", dto.Username);
            Assert.Equal("existing@example.com", dto.Email);
        }

        [Fact]
        public async Task PatchCurrentUser_UpdatesUser()
        {
            await using var factory = new CustomWebApplicationFactory<Program>();
            var client = factory.CreateAuthenticatedClient("Chrome");

            var patchDto = new UpdateAccountDto
            {
                Username = "updated",
                Email = "updated@example.com",
            };
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(patchDto);

            // 2️⃣ Calculer signature
            var url = "api/account/me";

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Vérifier mise à jour
            var get = await client.GetAsync("api/account/me");
            var dto = await get.Content.ReadFromJsonAsync<ReadAccountDto>();

            Assert.NotNull(dto);
            Assert.Equal("updated", dto.Username);
            Assert.Equal("updated@example.com", dto.Email);
        }

        [Fact]
        public async Task PatchCurrentUser_UpdatesPassword()
        {
            await using var factory = new CustomWebApplicationFactory<Program>();
            var client = factory.CreateAuthenticatedClient("Chrome");

            var passwordService = new PasswordService();
            var dto = new UpdateAccountDto
            {
                ActualPassword = "hashed-password",
                NewPassword = "NewPassword123!"
            };

            // 1️⃣ Serialize le DTO une seule fois
            var userId = Guid.Parse("d3f1a9e2-5b6c-4f7d-9a12-8c3e4b5f6789");
            var deviceName = "IntegrationTestDevice";

            // 1️⃣ Serialize le DTO une seule fois
            var jsonBody = JsonSerializer.Serialize(dto);

            // 2️⃣ Calculer signature
            var url = "api/account/me";

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Device-Name", deviceName);

            var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Reconnexion avec nouveau mot de passe
            var loginResponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequestDto
            {
                Email = "existing@example.com",
                Password = "NewPassword123!",
                DeviceName = "IntegrationTestDevice",
                Fingerprint = "IntegrationTestFingerprint"
            });

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteCurrentUser_DeletesUser()
        {
            await using var factory = new CustomWebApplicationFactory<Program>();
            var client = factory.CreateAuthenticatedClient("Chrome");

            var deleteResponse = await client.DeleteAsync("api/account/me");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await client.GetAsync("api/account/me");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
