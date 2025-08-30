using ArzenalStoreApi.Data;
using ArzenalStoreApi.Services.PasswordService;
using Microsoft.EntityFrameworkCore;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreSharedDto.DTOs.AccountDto;

namespace ArzenalStoreApi.Services.UserService
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

        public async Task DeleteAsync(string userId)
        {
            var id = userId != null ? Guid.Parse(userId) : Guid.Empty;
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id) ?? throw new UnauthorizedAccessException("Utilisateur introuvable");
            _dbContext.Users.Remove(user); // suppression de l'utilisateur
            await _dbContext.SaveChangesAsync(); // Sauvegarde des modifications dans la base de données
        }

        public async Task PatchAsync(string userId, UpdateAccountDto updateAccountDto)
        {
            var id = userId != null ? Guid.Parse(userId) : Guid.Empty;
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id) ?? throw new UnauthorizedAccessException("Utilisateur introuvable");
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
                if (!_passwordService.Verify(updateAccountDto.ActualPassword, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Mot de passe incorrecte"); // Mauvais utilisateur ou mot de passe
                }
                user.PasswordHash = _passwordService.Hash(updateAccountDto.NewPassword);
            }

            _dbContext.Users.Update(user); // Met à jour l'utilisateur
            await _dbContext.SaveChangesAsync(); // Sauvegarde des modifications dans la base de données 
        }
    }
}
