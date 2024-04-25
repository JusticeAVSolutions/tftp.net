using System.IO;
using System.Threading;
using System.Net;

namespace Tftp.Net.UnitTests;

public class TftpClientServer_Test
{
    public byte[] DemoData = { 1, 2, 3 };
    private bool TransferHasFinished = false;

    [Fact]
    public void ClientsReadsFromServer()
    {
        using (TftpServer server = new TftpServer(new IPEndPoint(IPAddress.Loopback, 10069)))
        {
            server.OnReadRequest += server_OnReadRequest;
            server.Start();

            TftpClient client = new TftpClient(new IPEndPoint(IPAddress.Loopback, 10069));
            using (ITftpTransfer transfer = client.Download("Demo File"))
            {
                MemoryStream ms = new MemoryStream();
                transfer.OnFinished += transfer_OnFinished;
                transfer.Start(ms);

                Thread.Sleep(500);
                Assert.True(TransferHasFinished);
            }
        }
    }

    private void transfer_OnFinished(ITftpTransfer transfer)
    {
        TransferHasFinished = true;
    }

    private void server_OnReadRequest(ITftpTransfer transfer, EndPoint client)
    {
        transfer.Start(new MemoryStream(DemoData));
    }
}