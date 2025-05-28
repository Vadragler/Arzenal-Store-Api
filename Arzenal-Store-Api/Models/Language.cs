namespace ArzenalStoreApi.Models
{
    public class Language
    {
        public ICollection<AppLanguage>? AppLanguages { get; set; }

        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
