namespace ArzenalStoreApi.Services.InviteService
{
    public interface IInviteService
    {
        Task ValidateInviteAsync(string? token);
        Task<string> CreateInviteAsync(string email);
        Task UseInviteAsync(Guid token);
    }
}
