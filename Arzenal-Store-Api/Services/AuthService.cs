using Arzenal.Shared.Dtos.DTOs.RegisterDto;
using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArzenalStoreApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(AuthDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequestDto request)
        {
            // Vérifie si le token d'invitation est valide
            var invite = await _dbContext.InviteTokens
                .FirstOrDefaultAsync(t => t.Token == request.Token && !t.Used && t.ExpiresAt > DateTime.UtcNow);

            if (invite == null)
            {
                return (false, "Token invalide ou déjà utilisé.");
            }

            // Vérifie si un utilisateur avec le même email ou nom d'utilisateur existe déjà
            var existingUser = await _dbContext.Users
                .AnyAsync(u => u.Email == request.Email || u.Username == request.Username);

            if (existingUser)
            {
                return (false, "Email ou nom d'utilisateur déjà utilisé.");
            }

            // Crée un nouvel utilisateur
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password)
            };

            _dbContext.Users.Add(user);

            // Marque le token comme utilisé
            invite.Used = true;


            await _dbContext.SaveChangesAsync();

            return (true, "Inscription réussie.");
        }

        public async Task<string?> AuthenticateAsync(string email, string password)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;  // Mauvais utilisateur ou mot de passe
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpiryHours"] ?? "1")), // Utilise une valeur configurable
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<(string? username, string email)> FindByIdAsync(string? userId)
        {
            var id = userId != null ? Guid.Parse(userId) : Guid.Empty;
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return (null, string.Empty); // Retourne des valeurs par défaut si l'utilisateur n'est pas trouvé
            }

            return (user.Username, user.Email);
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            var id = userId != null ? Guid.Parse(userId) : Guid.Empty;
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false; // Retourne des valeurs par défaut si l'utilisateur n'est pas trouvé
            }
            try
            {
                _dbContext.Users.Remove(user); // suppression de l'utilisateur
                await _dbContext.SaveChangesAsync(); // Sauvegarde des modifications dans la base de données

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PatchAsync(string userId, string username, string email, string actualpassword, string newpassword)
        {
            var id = userId != null ? Guid.Parse(userId) : Guid.Empty;
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false; // Retourne des valeurs par défaut si l'utilisateur n'est pas trouvé
            }
            try
            {
                if (!string.IsNullOrEmpty(username))
                {
                    user.Username = username;
                }
                if (!string.IsNullOrEmpty(email))
                {
                    user.Email = email;
                }
                if (!string.IsNullOrEmpty(newpassword))
                {
                    if (user == null || !BCrypt.Net.BCrypt.Verify(actualpassword, user.PasswordHash))
                    {
                        return false;  // Mauvais utilisateur ou mot de passe
                    }
                    user.PasswordHash = HashPassword(newpassword);
                }

                _dbContext.Users.Update(user); // Met à jour l'utilisateur
                await _dbContext.SaveChangesAsync(); // Sauvegarde des modifications dans la base de données

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
