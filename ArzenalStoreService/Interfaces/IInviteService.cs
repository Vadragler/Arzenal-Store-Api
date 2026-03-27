namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IInviteService
    {
        Task ValidateInviteAsync(string? token);
        Task<string> CreateInviteAsync(string email);
        Task UseInviteAsync(string token);
    }
}
