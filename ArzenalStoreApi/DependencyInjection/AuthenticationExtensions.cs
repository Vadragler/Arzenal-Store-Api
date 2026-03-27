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
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "CookieJwt";
                options.DefaultChallengeScheme = "CookieJwt";

            })
            .AddJwtBearer("CookieJwt", options =>
            {
                var key = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
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
                var key = Encoding.UTF8.GetBytes(configuration["Jwt-App-Login:SecretKey"]);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = ClaimTypes.Role,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt-App-Login:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = "app-login",
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Déplacer la configuration des policies ici
            services.AddAuthorization(options =>
            {
                // ⚠️ Pas de RequireRole ici
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireRole("Admin")
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
                })

                .AddPolicy("ArzenalAuth", policy =>
                {
                    policy.AuthenticationSchemes.Add("CookieJwt");
                    policy.RequireClaim(JwtRegisteredClaimNames.Aud, "ArzenalAuth");
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
                });


            return services;
        }
    }
     
}
