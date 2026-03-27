namespace Arzenal.Store.Api.Domain.Models
{
    public class Tag
    {
        public ICollection<AppTag>? AppTags { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
    }
}
