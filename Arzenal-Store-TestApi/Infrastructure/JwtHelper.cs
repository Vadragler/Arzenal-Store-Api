using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Arzenal.Store.Api.TestIntegration.Infrastructure
{
    public static class JwtHelper
    {
        public static string GenerateTestJwt(IConfiguration configuration, Guid userId, string email)
        {
            var audiences = new[] { "web", "ArzenalStoreManager" };
               
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, "Admin"),
            };

            // ⚠ ajoute un claim aud pour chaque audience
            foreach (var aud in audiences)
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, aud));

            // Le paramètre audience peut être null ou l’un des auds
            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: audiences.FirstOrDefault(), // sert juste pour le constructeur
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
