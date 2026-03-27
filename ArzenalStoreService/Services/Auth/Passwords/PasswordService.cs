using Arzenal.Store.Api.Service.Interfaces;

namespace Arzenal.Store.Api.Service.Services.Auth.Passwords
{
    public class PasswordService : IPasswordService
    {
        public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
        public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
