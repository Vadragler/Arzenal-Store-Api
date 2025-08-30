using ArzenalStoreApi.Data;
using ArzenalStoreApi.Exceptions;
using ArzenalStoreApi.Models;
using ArzenalStoreApi.Services.PasswordService;
using ArzenalStoreApi.Services.Token;
using ArzenalStoreSharedDto.DTOs.AuthDto;
using Microsoft.EntityFrameworkCore;

namespace ArzenalStoreApi.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _dbContext;
        private readonly IRefreshTokenService _tokenService;
        private readonly IJwtService _JwtService;
        private readonly IPasswordService _passwordService;

        public AuthService(
            AuthDbContext dbContext,
            IRefreshTokenService tokenService,
            IJwtService jwtService,
            IPasswordService passwordService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _JwtService = jwtService;
            _passwordService = passwordService;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto request)
        {

            // Vérifie si le token d'invitation est valide
            var invite = await _dbContext.InviteTokens
                .FirstOrDefaultAsync(t => t.Token == request.Token && !t.Used && t.ExpiresAt > DateTime.UtcNow);

            if (invite == null)
            {
                throw new ValidationException("Token invalide ou déjà utilisé.");
            }

            // Vérifie si un utilisateur avec le même email ou nom d'utilisateur existe déjà
            var existingUser = await _dbContext.Users
                .AnyAsync(u => u.Email == request.Email || u.Username == request.Username);

            if (existingUser)
            {
                throw new DuplicateException("Email ou nom d'utilisateur déjà utilisé.");
            }

            // Crée un nouvel utilisateur
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordService.Hash(request.Password)
            };

            _dbContext.Users.Add(user);

            // Marque le token comme utilisé
            invite.Used = true;


            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<(string? accessToken,string? refreshToken)> AuthenticateAsync(string email, string password, CreateRefreshTokenDto dto)
        {

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null || !_passwordService.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var accessToken = await _JwtService.GenerateJwtTokenAsync(user.Email);

            dto.UserId = user.Id;
            var refreshToken = await _tokenService.GenerateRefreshToken(dto);

            if (accessToken == null || refreshToken == null)
            {
                throw new UnauthorizedAccessException("token invalide");
            }
            return (accessToken, refreshToken);
        }

        public async Task LogoutAsync(string? refreshToken, string userIdStr)
        {
            var success = await _tokenService.RevokeRefreshTokenAsync(refreshToken, userIdStr);
            if (!success)
                throw new NotFoundException("Token introuvable, déjà révoqué, ou ne vous appartient pas.");
        }
    }
}
