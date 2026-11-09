namespace Arzenal.Store.Api.Domain.Models
{
    public class Release
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AppId { get; set; }
        public App App { get; set; } = null!;
        public required string Version { get; set; }
        public required string Status { get; set; } = "Draft";
        public string Channel { get; set; } = "stable";
        public string? Changelog { get; set; }
        public ICollection<AppArtifact> Artifacts { get; set; } = new List<AppArtifact>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }
    }
}
