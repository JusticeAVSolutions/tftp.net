using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests;

public class StartOutgoingRead_Test
{
    private TransferStub transfer;

    public StartOutgoingRead_Test()
    {
        transfer = new TransferStub();
        transfer.SetState(new StartOutgoingRead());
    }

    [Fact]
    public void CanCancel()
    {
        transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.IsType<Closed>(transfer.State);
    }

    [Fact]
    public void IgnoresCommands()
    {
        transfer.OnCommand(new Error(5, "Hallo Welt"));
        Assert.IsType<StartOutgoingRead>(transfer.State);
    }

    [Fact]
    public void CanStart()
    {
        transfer.Start(new MemoryStream());
        Assert.True(transfer.CommandWasSent(typeof(ReadRequest)));
        Assert.IsType<SendReadRequest>(transfer.State);
    }
}