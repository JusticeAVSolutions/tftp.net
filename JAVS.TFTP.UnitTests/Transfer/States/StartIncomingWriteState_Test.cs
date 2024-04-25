using System.Linq;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests.Transfer.States;

public class StartIncomingWriteState_Test
{
    private TransferStub transfer;

    public StartIncomingWriteState_Test()
    {
        transfer = new TransferStub();
        transfer.SetState(new StartIncomingWrite(new TransferOption[0]));
    }

    [Fact]
    public void CanCancel()
    {
        transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.True(transfer.CommandWasSent(typeof(Error)));
        Assert.IsType<Closed>(transfer.State);
    }

    [Fact]
    public void IgnoresCommands()
    {
        transfer.OnCommand(new Error(5, "Hallo Welt"));
        Assert.IsType<StartIncomingWrite>(transfer.State);
    }

    [Fact]
    public void CanStartWithoutOptions()
    {
        transfer.Start(new MemoryStream(new byte[50000]));

        Assert.True(transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.IsType<AcknowledgeWriteRequest>(transfer.State);
    }

    [Fact]
    public void CanStartWithOptions()
    {
        transfer.SetState(new StartIncomingWrite(new TransferOption[] { new TransferOption("blksize", "999") }));
        Assert.Equal(999, transfer.BlockSize);
        transfer.Start(new MemoryStream(new byte[50000]));
        OptionAcknowledgement cmd = (OptionAcknowledgement)transfer.SentCommands.Last();
        cmd.Options.Contains(new TransferOption("blksize", "999"));
        Assert.IsType<SendOptionAcknowledgementForWriteRequest>(transfer.State);
    }
}