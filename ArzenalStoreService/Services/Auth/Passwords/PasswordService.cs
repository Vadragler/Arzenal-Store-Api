using Arzenal.Store.Api.Service.Interfaces;

using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Arzenal.Store.Api.Service.Services.Auth.Passwords
{
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int MemorySizeKiB = 64 * 1024;
        private const int Iterations = 3;
        private const int DegreeOfParallelism = 1;
        private const string ParameterEncoding = "m=65536,t=3,p=1";

        public string Hash(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = DeriveHash(password, salt);

            return $"$argon2id$v=19${ParameterEncoding}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || !TryParseHash(hash, out var salt, out var expectedHash))
                return false;

            var actualHash = DeriveHash(password, salt);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        private static byte[] DeriveHash(string password, byte[] salt)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                MemorySize = MemorySizeKiB,
                Iterations = Iterations,
                DegreeOfParallelism = DegreeOfParallelism
            };

            return argon2.GetBytes(HashSize);
        }

        private static bool TryParseHash(string? encodedHash, out byte[] salt, out byte[] hash)
        {
            salt = Array.Empty<byte>();
            hash = Array.Empty<byte>();

            if (string.IsNullOrWhiteSpace(encodedHash))
                return false;

            var parts = encodedHash.Split('$', StringSplitOptions.None);
            if (parts.Length != 6 ||
                parts[0] != string.Empty ||
                !string.Equals(parts[1], "argon2id", StringComparison.Ordinal) ||
                !string.Equals(parts[2], "v=19", StringComparison.Ordinal) ||
                !string.Equals(parts[3], ParameterEncoding, StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                salt = Convert.FromBase64String(parts[4]);
                hash = Convert.FromBase64String(parts[5]);
                return salt.Length == SaltSize && hash.Length == HashSize;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
