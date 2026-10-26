namespace ArzenalStoreInfrastructure.Configurations
{
    public class StorageSettings
    {
        public string AppFilesPath { get; set; } = string.Empty;
        public long MaxUploadBytes { get; set; } = 268_435_456;
        public bool RequireMalwareScan { get; set; }
    }
}

