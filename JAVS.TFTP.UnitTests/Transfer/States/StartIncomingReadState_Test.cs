using System;
using System.Linq;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests;

public class StartIncomingReadState_Test
{
    private TransferStub transfer;

    public StartIncomingReadState_Test()
    {
        transfer = new TransferStub();
        transfer.SetState(new StartIncomingRead(new TransferOption[] { new TransferOption("tsize", "0") }));
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
        Assert.IsType<StartIncomingRead>(transfer.State);
    }

    [Fact]
    public void CanStartWithoutOptions()
    {
        transfer.SetState(new StartIncomingRead(new TransferOption[0]));
        transfer.Start(new MemoryStream(new byte[50000]));
        Assert.IsType<Sending>(transfer.State);
    }

    [Fact]
    public void CanStartWithOptions()
    {
        //Simulate that we got a request for a option
        transfer.SetState(new StartIncomingRead(new TransferOption[] { new TransferOption("blksize", "999") }));
        Assert.Equal(999, transfer.BlockSize);
        transfer.Start(new MemoryStream(new byte[50000]));
        Assert.IsType<SendOptionAcknowledgementForReadRequest>(transfer.State);
        OptionAcknowledgement cmd = (OptionAcknowledgement)transfer.SentCommands.Last();
        cmd.Options.Contains(new TransferOption("blksize", "999"));
    }

    [Fact]
    public void FillsTransferSizeIfPossible()
    {
        transfer.ExpectedSize = 123;
        transfer.Start(new StreamThatThrowsExceptionWhenReadingLength());
        Assert.True(WasTransferSizeOptionRequested());
    }

    [Fact]
    public void FillsTransferSizeFromStreamIfPossible()
    {
        transfer.Start(new MemoryStream(new byte[] { 1 }));
        Assert.True(WasTransferSizeOptionRequested());
    }

    [Fact]
    public void DoesNotFillTransferSizeWhenNotAvailable()
    {
        transfer.Start(new StreamThatThrowsExceptionWhenReadingLength());
        Assert.False(WasTransferSizeOptionRequested());
    }

    private bool WasTransferSizeOptionRequested()
    {
        OptionAcknowledgement oack = transfer.SentCommands.Last() as OptionAcknowledgement;
        return oack != null && oack.Options.Any(x => x.Name == "tsize");
    }

    private class StreamThatThrowsExceptionWhenReadingLength : MemoryStream
    {
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }
    }
}