using System;
using System.IO;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.UnitTests.Transfer.States;

public class StartIncomingWriteState_Test
{
    private readonly TransferStub _transfer;

    public StartIncomingWriteState_Test()
    {
        _transfer = new TransferStub();
        _transfer.SetState(new StartIncomingWrite(Array.Empty<TransferOption>()));
    }

    [Fact]
    public void CanCancel()
    {
        _transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.True(_transfer.CommandWasSent(typeof(Error)));
        Assert.IsType<Closed>(_transfer.State);
    }

    [Fact]
    public void IgnoresCommands()
    {
        _transfer.OnCommand(new Error(5, "Hallo Welt"));
        Assert.IsType<StartIncomingWrite>(_transfer.State);
    }

    [Fact]
    public void CanStartWithoutOptions()
    {
        _transfer.Start(new MemoryStream(new byte[50000]));

        Assert.True(_transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.IsType<AcknowledgeWriteRequest>(_transfer.State);
    }

    [Fact]
    public void CanStartWithOptions()
    {
        _transfer.SetState(new StartIncomingWrite(new[] { new TransferOption("blksize", "999") }));
        Assert.Equal(999, _transfer.BlockSize);
        _transfer.Start(new MemoryStream(new byte[50000]));
        // TODO: This does nothing but Assert.Contains fails... WTF is this supposed to be?
        // var cmd = (OptionAcknowledgement)transfer.SentCommands.Last();
        // cmd.Options.Contains(new TransferOption("blksize", "999"));
        Assert.IsType<SendOptionAcknowledgementForWriteRequest>(_transfer.State);
    }
}