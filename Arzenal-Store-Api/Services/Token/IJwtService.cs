namespace ArzenalStoreApi.Services.Token
{
    public interface IJwtService
    {
        Task<string> GenerateJwtTokenAsync(string email);
    }
}
