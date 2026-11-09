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
            if (!_settings.Enabled || _settings.TimeoutSeconds is < 1 or > 300)
                throw new ServiceUnavailableException("Le scanner est indisponible ou mal configuré.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_settings.TimeoutSeconds));
            try
            {
                await ScanCoreAsync(filePath, timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new ServiceUnavailableException("L'analyse antivirus a dépassé son délai.");
            }
            catch (Exception exception) when (exception is SocketException or IOException)
            {
                throw new ServiceUnavailableException("Le scanner antivirus est inaccessible.");
            }
        }

        private async Task ScanCoreAsync(string filePath, CancellationToken cancellationToken)
        {
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
            var responseLength = 0;
            while (responseLength < responseBuffer.Length)
            {
                var read = await network.ReadAsync(responseBuffer.AsMemory(responseLength), cancellationToken);
                if (read == 0) break;
                responseLength += read;
                var terminator = Array.IndexOf(responseBuffer, (byte)0, 0, responseLength);
                if (terminator < 0) continue;
                var response = Encoding.UTF8.GetString(responseBuffer, 0, terminator);
                if (response.EndsWith(" FOUND", StringComparison.Ordinal))
                    throw new ValidationException("Le fichier a été rejeté par l'analyse antivirus.");
                if (response == "stream: OK") return;
                break;
            }
            throw new ServiceUnavailableException("Le scan antivirus n'a pas retourné de résultat valide.");
        }
    }
}
