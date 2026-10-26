using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Arzenal.Store.Api.Service.Services.Auth.Tokens
{
    public sealed class HmacRefreshTokenHasher : IRefreshTokenHasher
    {
        private readonly byte[] _key;

        public HmacRefreshTokenHasher(IConfiguration configuration)
        {
            var encodedKey = configuration["RefreshToken:HashKey"];
            if (string.IsNullOrWhiteSpace(encodedKey))
                throw new InvalidOperationException("La clé de hachage des refresh tokens n'est pas configurée.");

            try
            {
                _key = Convert.FromBase64String(encodedKey);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("La clé de hachage des refresh tokens doit être encodée en Base64.", ex);
            }

            if (_key.Length < 32)
                throw new InvalidOperationException("La clé de hachage des refresh tokens doit contenir au moins 32 octets.");
        }

        public string Hash(string token)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            return Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(token)));
        }
    }
}
