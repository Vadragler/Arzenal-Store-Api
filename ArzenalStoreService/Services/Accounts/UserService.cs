using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Dto.DTOs.AccountDto;
using Microsoft.EntityFrameworkCore;
using Arzenal.Dto.DTOs.AccountDto;
using Arzenal.Store.Api.Infrastructure.Data;


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
            var id = userId != null ? Guid.Parse(userId) : Guid.Empty;
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
            var id = userId != null ? Guid.Parse(userId) : Guid.Empty;
            if (id == Guid.Empty)
                throw new ValidationException("Identifiant Invalide");

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id) ?? throw new NotFoundException("Utilisateur introuvable");
            _dbContext.Users.Remove(user); // suppression de l'utilisateur
            await _dbContext.SaveChangesAsync(); // Sauvegarde des modifications dans la base de données
        }

        public async Task PatchAsync(string? userId, UpdateAccountDto updateAccountDto)
        {
            var id = userId != null ? Guid.Parse(userId) : Guid.Empty;
            if (id == Guid.Empty)
                throw new ValidationException("Identifiant Invalide");

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id) ?? throw new NotFoundException("Utilisateur introuvable");
            if (!string.IsNullOrEmpty(updateAccountDto.Username))
            {
                user.Username = updateAccountDto.Username;
            }
            if (!string.IsNullOrEmpty(updateAccountDto.Email))
            {
                user.Email = updateAccountDto.Email;
            }
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
