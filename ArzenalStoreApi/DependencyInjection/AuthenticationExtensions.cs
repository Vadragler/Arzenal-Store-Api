using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArzenalStoreApi.DependencyInjection
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSecret = GetRequiredSecret(configuration, "Jwt:SecretKey");
            var appLoginSecret = GetRequiredSecret(configuration, "Jwt-App-Login:SecretKey");
            var jwtIssuer = GetRequiredValue(configuration, "Jwt:Issuer");
            var appLoginIssuer = GetRequiredValue(configuration, "Jwt-App-Login:Issuer");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "CookieJwt";
                options.DefaultChallengeScheme = "CookieJwt";

            })
            .AddJwtBearer("CookieJwt", options =>
            {
                var key = Encoding.UTF8.GetBytes(jwtSecret);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudiences = new[]
                    {
                        "web",
                        "ArzenalStoreManager"
                    },
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("authToken", out var cookieToken))
                            context.Token = cookieToken;

                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer("HeaderJwt", options =>
            {
                var key = Encoding.UTF8.GetBytes(appLoginSecret);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = ClaimTypes.Role,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = appLoginIssuer,
                    ValidateAudience = true,
                    ValidAudience = "app-login",
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Déplacer la configuration des policies ici
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddAuthorizationBuilder()

                .AddPolicy("Admin", policy =>
                {
                    policy.AuthenticationSchemes.Add("CookieJwt");
                    policy.RequireRole("Admin");
                })

                .AddPolicy("Web", policy =>
                {
                    policy.AuthenticationSchemes.Add("CookieJwt");
                    policy.RequireClaim(JwtRegisteredClaimNames.Aud, "web");
                    policy.RequireRole("User", "Admin");
                })

                .AddPolicy("ArzenalAuth", policy =>
                {
                    policy.AuthenticationSchemes.Add("CookieJwt");
                    policy.RequireClaim(JwtRegisteredClaimNames.Aud, "ArzenalAuth");
                    policy.RequireRole("User", "Admin");
                })

                .AddPolicy("ArzenalStoreManager", policy =>
                {
                    policy.AuthenticationSchemes.Add("CookieJwt");
                    policy.RequireClaim(JwtRegisteredClaimNames.Aud, "ArzenalStoreManager");
                    policy.RequireRole("Admin");
                })

                .AddPolicy("AppLogin", policy =>
                {
                    policy.AuthenticationSchemes.Add("HeaderJwt");
                    policy.RequireClaim(JwtRegisteredClaimNames.Aud, "app-login");
                    policy.RequireRole("User", "Admin");
                });


            return services;
        }

        private static string GetRequiredSecret(IConfiguration configuration, string key)
        {
            var value = GetRequiredValue(configuration, key);
            if (value.Length < 32 ||
                value.Contains("your_", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("change-me", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"La configuration {key} doit contenir un secret fort d'au moins 32 caractères.");
            }

            return value;
        }

        private static string GetRequiredValue(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"La configuration requise {key} est absente.");

            return value;
        }
    }

}
