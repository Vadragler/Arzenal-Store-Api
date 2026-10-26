namespace Arzenal.Store.Api.Domain.Models;

/// <summary>
/// Référence immuable vers un fichier stocké pour une application.
/// Le contenu reste sur le volume de stockage ; cette entité en conserve
/// l'identité métier et le chemin relatif contrôlé.
/// </summary>
public class AppArtifact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppId { get; set; }
    public App App { get; set; } = null!;
    public required string Type { get; set; }
    public required string Locator { get; set; }
    public string? Version { get; set; }
    public string? Platform { get; set; }
    public required string OriginalFileName { get; set; }
    public required string RelativePath { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
