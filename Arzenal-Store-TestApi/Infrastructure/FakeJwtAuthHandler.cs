using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure
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
            if (!Request.Headers.ContainsKey("X-True-Auth"))
            {

                // Récupère Fingerprint et DeviceName depuis les headers
                string? fingerprint = Request.Headers["Fingerprint"];
                string? deviceName = Request.Headers["X-Device-Name"];

                // Récupère le UserId et Email depuis des headers custom (pour tests)
                string userId = Request.Headers["X-UserId"].FirstOrDefault() ?? Guid.NewGuid().ToString();
                string email = Request.Headers["X-UserEmail"].FirstOrDefault() ?? "fake@example.com";

                // Détermine l'audience à partir du User-Agent (comme en production)
                string userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? string.Empty;
                string aud;
                if (userAgent.StartsWith("ArzenalStoreManager"))
                    aud = "ArzenalStoreManager";
                else if (userAgent.StartsWith("ArzenalStore-WPF"))
                    aud = "app-login";
                else
                    aud = "web";

                // Créé les claims
                var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("Fingerprint", fingerprint ?? ""),
                new Claim("DeviceName", deviceName ?? ""),
                new Claim(JwtRegisteredClaimNames.Aud, aud)
                };

                var identity = new ClaimsIdentity(claims, "Fake");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Fake");


                var authHeader = Request.Headers["Authorization"].ToString();

                if (authHeader == "Bearer INVALID_TOKEN")
                    return Task.FromResult(AuthenticateResult.Fail("Invalid token"));

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            else if (Request.Headers.ContainsKey("X-WPF-Auth"))
            {
                var configuration = Context.RequestServices.GetRequiredService<IConfiguration>();

                // → Lire le token dans Authorization: Bearer XXX
                var authHeader = Request.Headers["Authorization"].ToString();

                if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult(AuthenticateResult.Fail("Missing Bearer token"));

                var jwt = authHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    var key = Encoding.UTF8.GetBytes(configuration["Jwt-App-Login:SecretKey"]);
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var principal = tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt-App-Login:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = "app-login",
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var ticket = new AuthenticationTicket(principal, "Fake");
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                catch
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid JWT"));
                }
            }

            else
            {
                var configuration = Context.RequestServices.GetRequiredService<IConfiguration>();
                // Récupère le JWT depuis le cookie
                var jwt = Request.Cookies["authToken"];
                if (string.IsNullOrEmpty(jwt))
                    return Task.FromResult(AuthenticateResult.Fail("No auth token"));



                try
                {
                    var key = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]);
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var principal = tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudiences = new[]
                        {
                            "web",
                            "ArzenalStoreManager",
                            "ArzenalAuth"
                        },
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var ticket = new AuthenticationTicket(principal, "Fake");
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                catch
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid JWT"));
                }
            }
        }

        protected override Task InitializeHandlerAsync()
        {
            Options.TimeProvider = TimeProvider.System;
            return base.InitializeHandlerAsync();
        }
    }
}
