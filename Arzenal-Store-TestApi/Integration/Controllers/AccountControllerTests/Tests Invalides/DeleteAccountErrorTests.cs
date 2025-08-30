using ArzenalStoreApi.Controllers.User;
using ArzenalStoreApi.Services.UserService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.AccountControllerTests.Tests_Invalides
{
    public class DeleteAccountErrorTests
    {
        private readonly HttpClient _client;
        private readonly CustomAuthWebApplicationFactory<Program> _factory;

        public DeleteAccountErrorTests()
        {
            _factory = new CustomAuthWebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task DeleteCurrentUser_ReturnsUnauthorized_WhenDeleteFails()
        {
            var response = await _client.DeleteAsync("api/account/me");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
