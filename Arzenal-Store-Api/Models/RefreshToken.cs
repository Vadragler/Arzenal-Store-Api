namespace ArzenalStoreApi.Models
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public string Token { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public required string Fingerprint { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? CreatedByIp { get; set; }
        public required string DeviceName { get; set; }
        public string? UserAgent { get; set; }

        public bool IsActive => !IsRevoked && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);

        public User User { get; set; } = default!;
    }
}
