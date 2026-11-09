namespace ArzenalStoreInfrastructure.Configurations;

public class StoreDistributionSettings
{
    public string PublisherDisplayName { get; set; } = "Arzenal";
    // Clés publiques PEM uniquement. La clé privée de publication reste sur le PC de l'éditeur.
    public Dictionary<string, string> PublisherPublicKeys { get; set; } = new(StringComparer.Ordinal);
}
