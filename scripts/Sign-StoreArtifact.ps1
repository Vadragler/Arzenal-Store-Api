#requires -Version 7.5
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Path,
    [Parameter(Mandatory)][string]$CertificateThumbprint,
    [string]$PublisherKeyId = 'main'
)

$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'Ce script utilise le magasin de certificats Windows du développeur.' }
if ($CertificateThumbprint -notmatch '^[0-9A-Fa-f]{40}$') { throw 'Empreinte de certificat invalide.' }
$publisherCertificate = Get-Item -LiteralPath "Cert:\CurrentUser\My\$CertificateThumbprint"
if (-not $publisherCertificate.HasPrivateKey -or $publisherCertificate.NotAfter -le (Get-Date)) {
    throw 'La clé privée du certificat doit être disponible et le certificat non expiré.'
}
$publisherPrivateKey = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($publisherCertificate)
$artifactStream = [System.IO.File]::OpenRead((Resolve-Path -LiteralPath $Path).Path)
try {
    $artifactHash = [System.Security.Cryptography.SHA256]::HashData($artifactStream)
    $artifactSignature = $publisherPrivateKey.SignHash($artifactHash,
        [System.Security.Cryptography.HashAlgorithmName]::SHA256,
        [System.Security.Cryptography.RSASignaturePadding]::Pss)
    [pscustomobject]@{
        PublisherKeyId = $PublisherKeyId
        Sha256 = [Convert]::ToHexStringLower($artifactHash)
        Signature = [Convert]::ToBase64String($artifactSignature)
    }
}
finally {
    $artifactStream.Dispose()
    $publisherPrivateKey.Dispose()
}
