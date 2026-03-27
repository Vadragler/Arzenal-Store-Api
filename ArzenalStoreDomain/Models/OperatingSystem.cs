namespace Arzenal.Store.Api.Domain.Models
{
    public class OperatingSystem
    {
        public ICollection<AppOperatingSystem>? AppOperatingSystems { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
    }
}
