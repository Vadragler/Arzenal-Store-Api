#requires -Version 7.5
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'Ce script utilise le magasin de certificats Windows du développeur.' }

# La clé privée non exportable est conservée par Windows, jamais dans le dépôt ou le serveur.
$publisherCertificate = New-SelfSignedCertificate -Subject 'CN=Arzenal Distribution' `
    -CertStoreLocation 'Cert:\CurrentUser\My' -KeyAlgorithm RSA -KeyLength 3072 `
    -KeyExportPolicy NonExportable -KeyUsage DigitalSignature -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddYears(3)
# Recharger via le provider local, y compris si le module PKI utilise la compatibilité Windows PowerShell.
$publisherCertificate = Get-Item -LiteralPath (Join-Path 'Cert:\CurrentUser\My' $publisherCertificate.Thumbprint)
$publisherPublicKey = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPublicKey($publisherCertificate)
try {
    [pscustomobject]@{
        CertificateThumbprint = $publisherCertificate.Thumbprint
        PublicKeyPem = $publisherPublicKey.ExportSubjectPublicKeyInfoPem()
    }
}
finally { $publisherPublicKey.Dispose() }
