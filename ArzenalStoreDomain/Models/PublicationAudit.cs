namespace Arzenal.Store.Api.Domain.Models;

public class PublicationAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    // Les identifiants sont conservés même après suppression d'un brouillon.
    public Guid AppId { get; set; }
    public Guid? ReleaseId { get; set; }
    public Guid? ArtifactId { get; set; }
    public required string ActorId { get; set; }
    public required string Action { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
