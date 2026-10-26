namespace Arzenal.Store.Api.Service.Interfaces
{
    public interface IFileScanner
    {
        Task ScanAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
