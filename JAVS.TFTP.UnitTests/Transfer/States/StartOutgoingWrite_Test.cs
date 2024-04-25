using System;
using System.Linq;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests.Transfer.States;

public class StartOutgoingWrite_Test
{
    private TransferStub transfer;

    public StartOutgoingWrite_Test()
    {
        transfer = new TransferStub();
        transfer.SetState(new StartOutgoingWrite());
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
        Assert.IsType<StartOutgoingWrite>(transfer.State);
    }

    [Fact]
    public void CanStart()
    {
        transfer.Start(new MemoryStream());
        Assert.IsType<SendWriteRequest>(transfer.State);
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
        WriteRequest wrq = (WriteRequest)transfer.SentCommands.Last();
        return wrq.Options.Any(x => x.Name == "tsize");
    }

    private class StreamThatThrowsExceptionWhenReadingLength : MemoryStream
    {
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }
    }
}