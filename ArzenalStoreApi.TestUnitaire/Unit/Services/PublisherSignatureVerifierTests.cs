using System.Security.Cryptography;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Services.Store;
using ArzenalStoreInfrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services;

public class PublisherSignatureVerifierTests
{
    [Fact]
    public void Verify_AcceptsOnlyTheConfiguredPublisherAndTheExactFileHash()
    {
        using var key = RSA.Create(3072);
        var verifier = new PublisherSignatureVerifier(Options.Create(new StoreDistributionSettings
        {
            PublisherPublicKeys = new() { ["test"] = key.ExportSubjectPublicKeyInfoPem() }
        }));
        var hash = SHA256.HashData("original"u8);
        var signature = Convert.ToBase64String(key.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pss));

        verifier.Verify("test", signature, hash);
        var singleLineVerifier = new PublisherSignatureVerifier(Options.Create(new StoreDistributionSettings
        {
            PublisherPublicKeys = new() { ["test"] = key.ExportSubjectPublicKeyInfoPem().ReplaceLineEndings(" ") }
        }));
        singleLineVerifier.Verify("test", signature, hash);
        Assert.Throws<ValidationException>(() => verifier.Verify("unknown", signature, hash));
        Assert.Throws<ValidationException>(() => verifier.Verify("test", signature, SHA256.HashData("changed"u8)));
        Assert.Throws<ValidationException>(() => verifier.Verify("test", "invalid-base64", hash));
        var wrongPadding = Convert.ToBase64String(key.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        Assert.Throws<ValidationException>(() => verifier.Verify("test", wrongPadding, hash));
    }
}
