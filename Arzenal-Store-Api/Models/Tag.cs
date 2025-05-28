namespace ArzenalStoreApi.Models
{
    public class Tag
    {
        public ICollection<AppTag>? AppTags { get; set; }
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
