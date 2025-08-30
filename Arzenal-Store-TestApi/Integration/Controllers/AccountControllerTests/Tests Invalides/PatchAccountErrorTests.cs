using ArzenalStoreApi.Controllers.User;
using ArzenalStoreApi.Services.UserService;
using ArzenalStoreSharedDto.DTOs.AccountDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using TestArzenalStoreApi.Infrastructure;

namespace TestArzenalStoreApi.Integration.Controllers.AccountControllerTests.Tests_Invalides
{
    public class PatchAccountErrorTests
    {
        private readonly HttpClient _client;
        private readonly CustomAuthWebApplicationFactory<Program> _factory;

        public PatchAccountErrorTests()
        {
            _factory = new CustomAuthWebApplicationFactory<Program>();
            _client = _factory.CreateClient();

        }

        [Fact]
        public async Task PatchCurrentUser_ReturnsUnauthorized_WhenPatchFails()
        {
            _client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Fake");


            var dto = new UpdateAccountDto
            {
                Username = "newuser",
                Email = "new@email.com",
                ActualPassword = "oldpass",
                NewPassword = "newpass"
            };

            var response = await _client.PatchAsJsonAsync("/api/account/me", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);


        }
    }
}
