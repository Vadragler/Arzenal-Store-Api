namespace Arzenal.Store.Api.Domain.Models
{
    public class Categorie
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        
    }
}
