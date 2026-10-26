using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateJwtTokenAsync(Guid userId, List<string> audiences);
        Task<string> GenerateJwtTokenForAppAsync(Guid userId, HttpRequest request);
    }
}
