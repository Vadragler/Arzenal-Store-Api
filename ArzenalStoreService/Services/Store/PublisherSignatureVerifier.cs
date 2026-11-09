using System.Security.Cryptography;
using Arzenal.Store.Api.Service.Exceptions;
using ArzenalStoreInfrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Arzenal.Store.Api.Service.Services.Store;

public sealed class PublisherSignatureVerifier(IOptions<StoreDistributionSettings> settings)
{
    public void Verify(string keyId, string signature, byte[] sha256)
    {
        if (keyId.Length > 100 || signature.Length > 2048 ||
            !settings.Value.PublisherPublicKeys.TryGetValue(keyId, out var publicKey))
            throw new ValidationException("Éditeur de paquet inconnu.");
        try
        {
            if (publicKey.Contains("PRIVATE KEY", StringComparison.Ordinal))
                throw new InvalidOperationException("Seules des clés publiques de publication peuvent être configurées.");
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);
            if (rsa.KeySize < 3072 || !rsa.VerifyHash(sha256, Convert.FromBase64String(signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pss))
                throw new ValidationException("Signature de publication invalide.");
        }
        catch (Exception exception) when (exception is CryptographicException or FormatException or ArgumentException)
        {
            throw new ValidationException("Signature de publication invalide.");
        }
    }
}
