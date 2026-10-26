namespace Arzenal.Store.Api.Domain.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TokenHash { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public Guid FamilyId { get; set; } = Guid.NewGuid();
        public required string Fingerprint { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public DateTime AbsoluteExpiresAt { get; set; } = DateTime.UtcNow.AddDays(90);
        public bool IsRevoked { get; set; }
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
        public string? ReplacedByTokenHash { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedByIp { get; set; }
        public required string DeviceName { get; set; }
        public string? UserAgent { get; set; }
        public string Type { get; set; } = "Inconnu";

        public bool IsActive =>
            !IsRevoked &&
            (ExpiresAt == null || ExpiresAt > DateTime.UtcNow) &&
            AbsoluteExpiresAt > DateTime.UtcNow;

        public User User { get; set; } = default!;
    }
}
