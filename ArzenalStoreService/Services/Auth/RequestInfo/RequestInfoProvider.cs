using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.AuthDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.Service.Services.Auth.RequestInfo
{
    public class RequestInfoProvider : IRequestInfoProvider
    {
        private readonly AuthDbContext _dbContext;

        public RequestInfoProvider(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateRefreshTokenDto> GetRequestInfo(HttpContext context, LoginRequestDto request)
        {
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
            var createdByIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                            ?? context.Connection.RemoteIpAddress?.ToString();

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Utilisateur non trouvé");
            }

            Guid userId = user.Id; // type Guid

            return new CreateRefreshTokenDto
            {
                UserId = userId,
                DeviceName = request.DeviceName,
                Fingerprint = request.Fingerprint,
                UserAgent = userAgent ?? "Unknown User-Agent",
                CreatedByIp = createdByIp
            };
        }
    }
}
