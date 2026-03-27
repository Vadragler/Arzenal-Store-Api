namespace Arzenal.Store.Api.Domain.Models
{
    public class Language
    {
        public ICollection<AppLanguage>? AppLanguages { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
    }
}
