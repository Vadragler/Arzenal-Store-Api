using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace TestArzenalStoreApi.Infrastructure
{
    public class FakeJwtAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public FakeJwtAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] {
                                new Claim(ClaimTypes.Name, "TestUser"),
                                new Claim(ClaimTypes.Role, "Admin") // Ajoute d'autres claims si nécessaire
                            };
            var identity = new ClaimsIdentity(claims, "Fake");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Fake");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override Task InitializeHandlerAsync()
        {
            Options.TimeProvider = TimeProvider.System; // Utilisation de TimeProvider
            return base.InitializeHandlerAsync();
        }
    }
}