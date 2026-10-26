namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IRefreshTokenHasher
    {
        string Hash(string token);
    }
}
