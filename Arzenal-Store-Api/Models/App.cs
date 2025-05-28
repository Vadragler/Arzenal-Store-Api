namespace ArzenalStoreApi.Models
{
    public class App
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required string Version { get; set; }
        public required string FilePath { get; set; }
        public string? Description { get; set; }
        public bool IsVisible { get; set; }
        public string? Icone { get; set; }   
        public int CategoryId { get; set; } // Propriété pour l'identifiant de la catégorie

        public Categorie? Category { get; set; }
        public DateTime ReleaseDate { get; set; } = DateTime.Now;  // Date de publication
        public DateTime? LastUpdated { get; set; } = DateTime.Now; // Dernière mise à jour
        public long AppSize { get; set; }  // Taille de l'application en octets
        public string? Requirements { get; set; }  // Conditions requises (ex: version minimum de l'OS)

        public ICollection<AppLanguage>? AppLanguages { get; set; }

        public ICollection<AppTag>? AppTags { get; set; }

        public ICollection<AppOperatingSystem>? AppOperatingSystems { get; set; }
    }
}
