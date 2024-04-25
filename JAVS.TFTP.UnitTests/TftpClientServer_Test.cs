using System.IO;
using System.Threading;
using System.Net;

namespace Tftp.Net.UnitTests;

public sealed class TftpClientServer_Test
{
    [Fact]
    public void ClientsReadsFromServer()
    {
        var transferHasFinished = false;
        using (var server = new TftpServer(new IPEndPoint(IPAddress.Loopback, 10069)))
        {
            server.OnReadRequest += OnReadRequest;
            server.Start();

            var client = new TftpClient(new IPEndPoint(IPAddress.Loopback, 10069));
            using (var transfer = client.Download("Demo File"))
            {
                var ms = new MemoryStream();
                transfer.OnFinished += OnFinished;
                transfer.Start(ms);

                Thread.Sleep(500);
                Assert.True(transferHasFinished);
            }
        }

        return;

        void OnFinished(ITftpTransfer transfer) => transferHasFinished = true;
        void OnReadRequest(ITftpTransfer transfer, EndPoint client) => transfer.Start(new MemoryStream([1, 2, 3]));
    }
}