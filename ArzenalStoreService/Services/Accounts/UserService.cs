using Arzenal.Dto.DTOs.AccountDto;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace Arzenal.Store.Api.Service.Services.Accounts
{
    public class UserService : IUserService
    {
        private readonly AuthDbContext _dbContext;
        private readonly IPasswordService _passwordService;
        public UserService(AuthDbContext dbContext,IPasswordService passwordService)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
        }
        public async Task<ReadAccountDto> FindByIdAsync(string? userId)
        {
            var id = Guid.TryParse(userId, out var parsedId) ? parsedId : Guid.Empty;
            if (id == Guid.Empty)
                throw new ValidationException("Identifiant Invalide");

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id) ?? throw new NotFoundException("Utilisateur introuvable");
            var userDto = new ReadAccountDto
            {
                Username = user.Username,
                Email = user.Email
            };
            return userDto;
        }

        public async Task DeleteAsync(string? userId)
        {
            var id = Guid.TryParse(userId, out var parsedId) ? parsedId : Guid.Empty;
            if (id == Guid.Empty)
                throw new ValidationException("Identifiant Invalide");

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id) ?? throw new NotFoundException("Utilisateur introuvable");
            _dbContext.Users.Remove(user); // suppression de l'utilisateur
            await _dbContext.SaveChangesAsync(); // Sauvegarde des modifications dans la base de données
        }

        public async Task PatchAsync(string? userId, UpdateAccountDto updateAccountDto)
        {

            var id = Guid.TryParse(userId, out var parsedId) ? parsedId : Guid.Empty;
            if (id == Guid.Empty)
                throw new ValidationException("Identifiant Invalide");

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id) ?? throw new NotFoundException("Utilisateur introuvable");
            var username = string.IsNullOrWhiteSpace(updateAccountDto.Username)
                ? null
                : updateAccountDto.Username.Trim();
            var email = string.IsNullOrWhiteSpace(updateAccountDto.Email)
                ? null
                : updateAccountDto.Email.Trim();

            if (username is not null || email is not null)
            {
                var otherUsers = _dbContext.Users.Where(other => other.Id != id);
                if (username is not null)
                    otherUsers = otherUsers.Where(other => other.Username == username);
                if (email is not null)
                    otherUsers = username is null
                        ? otherUsers.Where(other => other.Email == email)
                        : _dbContext.Users.Where(other => other.Id != id &&
                            (other.Username == username || other.Email == email));

                var duplicate = await otherUsers.AnyAsync();
                if (duplicate)
                    throw new DuplicateException("Email ou nom d'utilisateur déjà utilisé.");
            }

            if (username is not null)
                user.Username = username;
            if (email is not null)
                user.Email = email;
            if (!string.IsNullOrEmpty(updateAccountDto.NewPassword))
            {
                if (updateAccountDto.ActualPassword == null)
                    throw new ValidationException("Mot de passe manquant");

                if (user.PasswordHash == null)
                    throw new InvalidOperationException("Mot de passe actuelle introuvable");
                if (!_passwordService.Verify(updateAccountDto.ActualPassword, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Mot de passe incorrecte");
                }
                user.PasswordHash = _passwordService.Hash(updateAccountDto.NewPassword);
            }

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
