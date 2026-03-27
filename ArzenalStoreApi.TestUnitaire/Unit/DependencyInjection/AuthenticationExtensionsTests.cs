using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ArzenalStoreApi.DependencyInjection;
using Xunit;

namespace ArzenalStoreApi.TestUnitaire.Unit.DependencyInjection
{
    public class AuthenticationExtensionsTests
    {
        [Fact]
        public void AddJwtAuthentication_RegistersPoliciesAndFallbackPolicy()
        {
            // Arrange
            var inMemory = new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-1234567890",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt-App-Login:SecretKey"] = "app-secret-key-1234567890",
                ["Jwt-App-Login:Issuer"] = "app-issuer",
                ["Jwt:ExpiryHours"] = "1"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemory)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            // Act
            services.AddJwtAuthentication(configuration);
            var provider = services.BuildServiceProvider();
            var authOptions = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

            // Assert: fallback policy requires authenticated user and role Admin
            Assert.NotNull(authOptions.FallbackPolicy);
            var fallbackReqTypeNames = authOptions.FallbackPolicy.Requirements.Select(r => r.GetType().Name).ToList();
            Assert.Contains("RolesAuthorizationRequirement", fallbackReqTypeNames);

            var adminPolicy = authOptions.GetPolicy("Admin");
            Assert.NotNull(adminPolicy);
            Assert.Contains("CookieJwt", adminPolicy.AuthenticationSchemes);
            // Ensure role requirement exists in Admin policy
            Assert.Contains(adminPolicy.Requirements, r => r.GetType().Name == "RolesAuthorizationRequirement");

            var webPolicy = authOptions.GetPolicy("Web");
            Assert.NotNull(webPolicy);
            Assert.Contains("CookieJwt", webPolicy.AuthenticationSchemes);
            Assert.Contains(webPolicy.Requirements, r => r.GetType().Name == "ClaimsAuthorizationRequirement");

            var arzenalAuth = authOptions.GetPolicy("ArzenalAuth");
            Assert.NotNull(arzenalAuth);
            Assert.Contains("CookieJwt", arzenalAuth.AuthenticationSchemes);
            Assert.Contains(arzenalAuth.Requirements, r => r.GetType().Name == "ClaimsAuthorizationRequirement");

            var storeManager = authOptions.GetPolicy("ArzenalStoreManager");
            Assert.NotNull(storeManager);
            Assert.Contains("CookieJwt", storeManager.AuthenticationSchemes);
            Assert.Contains(storeManager.Requirements, r => r.GetType().Name == "ClaimsAuthorizationRequirement");
            Assert.Contains(storeManager.Requirements, r => r.GetType().Name == "RolesAuthorizationRequirement");

            var appLogin = authOptions.GetPolicy("AppLogin");
            Assert.NotNull(appLogin);
            Assert.Contains("HeaderJwt", appLogin.AuthenticationSchemes);
            Assert.Contains(appLogin.Requirements, r => r.GetType().Name == "ClaimsAuthorizationRequirement");
        }
    }
}
