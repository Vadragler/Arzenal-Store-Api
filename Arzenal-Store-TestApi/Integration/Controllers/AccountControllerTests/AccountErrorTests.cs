using Arzenal.Dto.DTOs.AccountDto;
using System.Net;
using System.Net.Http.Json;
using Arzenal.Store.Api.TestIntegration.Infrastructure;

namespace Arzenal.Store.Api.TestIntegration.Integration.Controllers.AccountControllerTests
{
    public class AccountErrorTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly HttpClient _clientfake;
        public AccountErrorTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateAuthenticatedClient("Chrome");
            _clientfake = _factory.CreateFakeUserClient();
        }

        [Fact]
        public async Task GetUser_NonExistingUser_ReturnsNotFound()
        {
            var response = await _clientfake.GetAsync($"api/Account/me");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
        {
            var response = await _clientfake.DeleteAsync($"api/Account/me");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PatchUser_InvalidPassword_ReturnsUnauthorized()
        {
            var updateDto = new UpdateAccountDto
            {
                ActualPassword = "wrongpassword",
                NewPassword = "newpass"
            };

            var response = await _client.PatchAsJsonAsync($"api/Account/me", updateDto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
