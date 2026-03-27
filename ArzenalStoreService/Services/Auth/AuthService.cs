using Arzenal.Dto.DTOs.AuthDto;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Domain.Models.Requests;
using Arzenal.Store.Api.Infrastructure.Data;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;

namespace Arzenal.Store.Api.Service.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _dbContext;
        private readonly IRefreshTokenService _tokenService;
        private readonly IJwtService _JwtService;
        private readonly IPasswordService _passwordService;
        private readonly ICookieService _cookieService;
        private readonly IJwtCookieService _jwtCookieService;

        public AuthService(
            AuthDbContext dbContext,
            IRefreshTokenService tokenService,
            IJwtService jwtService,
            IPasswordService passwordService,
            ICookieService cookieService,
            IJwtCookieService jwtCookieService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _JwtService = jwtService;
            _passwordService = passwordService;
            _cookieService = cookieService;
            _jwtCookieService = jwtCookieService;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto request)
        {
            if(request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                throw new ValidationException("Données d'inscription invalides.");
            }

            // Vérifie si le token d'invitation est valide
            var invite = await _dbContext.InviteTokens
                .FirstOrDefaultAsync(t => t.Token == request.Token && !t.Used && t.ExpiresAt > DateTime.UtcNow);

            if (invite == null)
            {
                throw new UnauthorizedAccessException("Token invalide ou déjà utilisé.");
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

        public async Task<(string? accessToken, string? refreshToken)> AuthenticateAsync(HttpResponse Response, string email, string password, CreateRefreshTokenDto dto)
        {
            // 1️⃣ Vérifie l'utilisateur et le mot de passe
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null || !_passwordService.Verify(password, user.PasswordHash!))
                throw new UnauthorizedAccessException("Invalid email or password");

            dto.UserAgent = Response.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "";

            // 2️⃣ Génère l'accessToken
            List<string> audience = new List<string>()
                {
                GetAudienceFromUserAgent(dto.UserAgent),
                "ArzenalAuth"
                };
            var accessToken = await _JwtService.GenerateJwtTokenAsync(user.Id, audience);

            // 3️⃣ Cherche un refreshToken existant pour ce device + fingerprint
            var existingToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.UserId == user.Id &&
                    rt.Fingerprint == dto.Fingerprint &&
                    rt.DeviceName == dto.DeviceName);

            RefreshTokenCookieData refreshToken;

            if (existingToken != null)
            {
                // 4️⃣ Si un token existe, on le "rotate" : nouveau token + nouvelle expiration
                var newToken = await _tokenService.GenerateRefreshToken(dto);
                existingToken.Token = newToken.Token;
                existingToken.ExpiresAt = DateTime.UtcNow.AddDays(30);
                _dbContext.RefreshTokens.Update(existingToken);
                await _dbContext.SaveChangesAsync();
                refreshToken = new RefreshTokenCookieData
                {
                    Token = existingToken.Token,
                    UserId = existingToken.UserId
                };
            }
            else
            {
                // 5️⃣ Sinon, crée un nouveau refreshToken
                dto.UserId = user.Id;
                var newToken = await _tokenService.GenerateRefreshToken(dto);
                if (newToken == null)
                    throw new UnauthorizedAccessException("refresh token invalide");

                refreshToken = newToken;
            }

            if (accessToken == null || string.IsNullOrEmpty(refreshToken.Token))
                throw new UnauthorizedAccessException("token invalide");

            _cookieService.SetAuthCookies(Response, accessToken!, refreshToken);

            return (accessToken, refreshToken.Token);
        }

        public async Task RotateRefreshTokenAsync(HttpContext context)
        {
            var refreshToken = _cookieService.GetRefreshToken(context.Request);
            if (refreshToken != null)
            {
                var newRefreshToken = await _tokenService.RotateRefreshTokenAsync(context);
                if (newRefreshToken == null)
                    throw new UnauthorizedAccessException("Impossible de faire la rotation du token.");
                List<string> audience = new List<string>()
                {
                GetAudienceFromUserAgent(context.Request.Headers["User-Agent"].FirstOrDefault()),
                "ArzenalAuth"
                };
                var accessToken = await _JwtService.GenerateJwtTokenAsync(newRefreshToken.UserId, audience);
                _cookieService.SetAuthCookies(context.Response, accessToken, newRefreshToken);

            } else
                throw new UnauthorizedAccessException("Refresh token manquant.");
        }

        public async Task GenerateRefreshTokenAsync(HttpContext context, CreateRefreshTokenDto dto)
        {
            Guid userId = Guid.Empty;
            if (dto.UserId == Guid.Empty)
            {
                userId = _jwtCookieService.GetUserIdFromCookie();
                dto.UserId = userId;
            }
            var refreshToken = await _tokenService.GenerateRefreshToken(dto);
            if (dto.UserId == Guid.Empty || refreshToken == null)
                throw new UnauthorizedAccessException("Impossible de générer le token.");
            List<string> audience = new List<string>()
                {
                    GetAudienceFromUserAgent(context.Request.Headers["User-Agent"].FirstOrDefault()),
                    "ArzenalAuth"
                };
            var accessToken = await _JwtService.GenerateJwtTokenAsync(dto.UserId, audience);
            if (accessToken == null || string.IsNullOrEmpty(refreshToken.Token))
                throw new UnauthorizedAccessException("token invalide");
            _cookieService.SetAuthCookies(context.Response, accessToken, refreshToken);
        }

        private string GetAudienceFromUserAgent(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "web"; // valeur par défaut

            if (userAgent.StartsWith("ArzenalStoreManager"))
                // Doit correspondre à l'audience attendue par la policy 'ArzenalStoreManager'
                return "ArzenalStoreManager";

            if (userAgent.StartsWith("ArzenalStore-WPF"))
                // L'application WPF utilise l'audience 'app-login'
                return "app-login";

            return "web"; // fallback
        }

        public async Task RevokeRefreshTokenAsync(HttpContext context)
        {
            Guid userId = _jwtCookieService.GetUserIdFromCookie();
            var refreshToken = _cookieService.GetRefreshToken(context.Request);
            if (refreshToken != null)
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken.Token!, userId);
                _cookieService.DeleteAuthCookies(context);
            }
            else
                throw new UnauthorizedAccessException("Refresh token manquant.");
        }

        public async Task LogoutAsync(HttpContext context)
        {
            Guid userId = _jwtCookieService.GetUserIdFromCookie();
            var refreshToken = _cookieService.GetRefreshToken(context.Request);
            if (refreshToken != null)
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken.Token!, userId);
                _cookieService.DeleteAuthCookies(context);
                _dbContext.RefreshTokens.RemoveRange(_dbContext.RefreshTokens.Where(rt => rt.UserId == userId && rt.Token == refreshToken.Token));
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
