namespace Arzenal.Store.Api.Service.Models.Auth;

/// <summary>
/// Informations contrôlées par le serveur lors de la création d'un refresh token.
/// Cette classe ne doit jamais être liée directement à une requête HTTP cliente.
/// </summary>
public sealed record RefreshTokenCreationRequest(
    Guid UserId,
    string DeviceName,
    string Fingerprint,
    string? UserAgent,
    string? CreatedByIp);
