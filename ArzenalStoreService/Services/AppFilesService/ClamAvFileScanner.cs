using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Interfaces;
using ArzenalStoreInfrastructure.Configurations;
using Microsoft.Extensions.Options;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace Arzenal.Store.Api.Service.Services.AppFilesService
{
    public sealed class ClamAvFileScanner(IOptions<MalwareScanSettings> options) : IFileScanner
    {
        private readonly MalwareScanSettings _settings = options.Value;

        public async Task ScanAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled)
                throw new InvalidOperationException("Le scan antivirus est requis mais ClamAV n'est pas configuré.");

            using var client = new TcpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, cancellationToken);
            await using var network = client.GetStream();
            await network.WriteAsync("zINSTREAM\0"u8.ToArray(), cancellationToken);

            await using var input = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            var buffer = new byte[64 * 1024];
            while (true)
            {
                var read = await input.ReadAsync(buffer, cancellationToken);
                if (read == 0)
                    break;

                var length = new byte[sizeof(int)];
                BinaryPrimitives.WriteInt32BigEndian(length, read);
                await network.WriteAsync(length, cancellationToken);
                await network.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }

            await network.WriteAsync(new byte[sizeof(int)], cancellationToken);
            await network.FlushAsync(cancellationToken);

            var responseBuffer = new byte[1024];
            var responseLength = await network.ReadAsync(responseBuffer, cancellationToken);
            var response = Encoding.UTF8.GetString(responseBuffer, 0, responseLength);
            if (response.Contains("FOUND", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Le fichier a été rejeté par l'analyse antivirus.");
            if (!response.Contains("OK", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Le scan antivirus n'a pas retourné de résultat valide.");
        }
    }
}
