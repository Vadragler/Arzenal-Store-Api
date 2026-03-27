using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Dto.DTOs.AccountDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.Service.Services.Accounts
{
    public class DeviceService : IDeviceService
    {
        private readonly AuthDbContext _dbContext;
        private readonly IJwtCookieService _jwtCookieService;
        public DeviceService(AuthDbContext dbContext,IJwtCookieService jwtCookieService) 
        { 
            _dbContext = dbContext;
            _jwtCookieService = jwtCookieService;
        }

        public async Task<List<ReadDeviceDto>> GetAllDevice(HttpRequest Request)
        {
            
            var UserId = _jwtCookieService.GetUserIdFromCookie();
            var tokens = await _dbContext.RefreshTokens
        .Where(rt => rt.UserId == UserId)
        .ToListAsync();

            return tokens.Select(rt => new ReadDeviceDto
            {
                Id = rt.Id,
                DeviceName = rt.DeviceName,
                LastUsedIp = rt.CreatedByIp ?? "Inconnu",
                LastSeen = rt.LastUsed,
                IsActive = rt.IsActive,
                Type = rt.Type, // PC, Browser, Mobile
                IsCurrentDevice = _jwtCookieService.GetCurrentCookie(rt.Token)
            }).ToList();
        }

        public async Task<bool> RevokeDevice(Guid deviceId)
        {
            var UserId = _jwtCookieService.GetUserIdFromCookie();
            if (UserId == Guid.Empty)
                throw new UnauthorizedAccessException("Accès refuser");
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == deviceId && rt.UserId == UserId);
            if (token == null)
                throw new NotFoundException("appareil non trouvé");
            _dbContext.RefreshTokens.Remove(token);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
