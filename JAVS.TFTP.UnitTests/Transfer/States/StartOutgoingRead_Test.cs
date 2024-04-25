using System.IO;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.UnitTests.Transfer.States;

public class StartOutgoingRead_Test
{
    private readonly TransferStub _transfer;

    public StartOutgoingRead_Test()
    {
        _transfer = new TransferStub();
        _transfer.SetState(new StartOutgoingRead());
    }

    [Fact]
    public void CanCancel()
    {
        _transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.IsType<Closed>(_transfer.State);
    }

    [Fact]
    public void IgnoresCommands()
    {
        _transfer.OnCommand(new Error(5, "Hallo Welt"));
        Assert.IsType<StartOutgoingRead>(_transfer.State);
    }

    [Fact]
    public void CanStart()
    {
        _transfer.Start(new MemoryStream());
        Assert.True(_transfer.CommandWasSent(typeof(ReadRequest)));
        Assert.IsType<SendReadRequest>(_transfer.State);
    }
}