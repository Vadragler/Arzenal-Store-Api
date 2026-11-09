using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Arzenal.Store.Api.Service.Exceptions;
using Arzenal.Store.Api.Service.Services.AppFilesService;
using ArzenalStoreInfrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Services;

public class ClamAvFileScannerTests
{
    [Theory]
    [InlineData("stream: OK\0", "ok")]
    [InlineData("stream: Arzenal.Test FOUND\0", "infected")]
    [InlineData("garbage OK\0", "unavailable")]
    [InlineData("stream: ERROR\0", "unavailable")]
    [InlineData("stream: OK", "unavailable")]
    public async Task Scan_RequiresAnExactCompleteResponseEvenWhenTcpFragmentsIt(string reply, string expected)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var file = Path.Combine(Path.GetTempPath(), $"arzenal-scan-{Guid.NewGuid():N}.tmp");
        await File.WriteAllTextAsync(file, "safe scanner test");
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var server = Task.Run(async () =>
        {
            using var peer = await listener.AcceptTcpClientAsync(deadline.Token);
            await using var stream = peer.GetStream();
            var command = new byte[10];
            await stream.ReadExactlyAsync(command, deadline.Token);
            Assert.Equal("zINSTREAM\0", Encoding.UTF8.GetString(command));
            var length = new byte[4];
            while (true)
            {
                await stream.ReadExactlyAsync(length, deadline.Token);
                var count = BinaryPrimitives.ReadInt32BigEndian(length);
                if (count == 0) break;
                await stream.ReadExactlyAsync(new byte[count], deadline.Token);
            }
            foreach (var value in Encoding.UTF8.GetBytes(reply))
            {
                await stream.WriteAsync(new byte[] { value }, deadline.Token);
                await Task.Delay(1, deadline.Token);
            }
        }, deadline.Token);
        try
        {
            var scanner = new ClamAvFileScanner(Options.Create(new MalwareScanSettings { Enabled = true, Host = "127.0.0.1", Port = port, TimeoutSeconds = 5 }));
            if (expected == "ok") await scanner.ScanAsync(file);
            else if (expected == "infected") await Assert.ThrowsAsync<ValidationException>(() => scanner.ScanAsync(file));
            else await Assert.ThrowsAsync<ServiceUnavailableException>(() => scanner.ScanAsync(file));
            await server;
        }
        finally
        {
            listener.Stop();
            File.Delete(file);
        }
    }

    [Fact]
    public async Task DisabledScanner_RejectsUploads()
    {
        var scanner = new ClamAvFileScanner(Options.Create(new MalwareScanSettings()));
        await Assert.ThrowsAsync<ServiceUnavailableException>(() => scanner.ScanAsync("unused"));
    }
}
