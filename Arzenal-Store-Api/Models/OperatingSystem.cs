namespace ArzenalApi.Models
{
    public class OperatingSystem
    {
        public ICollection<AppOperatingSystem>? AppOperatingSystems { get; set; }
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
