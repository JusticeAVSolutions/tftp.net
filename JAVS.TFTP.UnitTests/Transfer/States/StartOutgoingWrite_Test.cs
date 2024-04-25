using System;
using System.IO;
using System.Linq;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.UnitTests.Transfer.States;

public class StartOutgoingWrite_Test
{
    private readonly TransferStub _transfer;

    public StartOutgoingWrite_Test()
    {
        _transfer = new TransferStub();
        _transfer.SetState(new StartOutgoingWrite());
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
        Assert.IsType<StartOutgoingWrite>(_transfer.State);
    }

    [Fact]
    public void CanStart()
    {
        _transfer.Start(new MemoryStream());
        Assert.IsType<SendWriteRequest>(_transfer.State);
    }

    [Fact]
    public void FillsTransferSizeIfPossible()
    {
        _transfer.ExpectedSize = 123;
        _transfer.Start(new StreamThatThrowsExceptionWhenReadingLength());
        Assert.True(WasTransferSizeOptionRequested());
    }

    [Fact]
    public void FillsTransferSizeFromStreamIfPossible()
    {
        _transfer.Start(new MemoryStream([1]));
        Assert.True(WasTransferSizeOptionRequested());
    }

    [Fact]
    public void DoesNotFillTransferSizeWhenNotAvailable()
    {
        _transfer.Start(new StreamThatThrowsExceptionWhenReadingLength());
        Assert.False(WasTransferSizeOptionRequested());
    }

    private bool WasTransferSizeOptionRequested()
    {
        var wrq = (WriteRequest)_transfer.SentCommands.Last();
        return wrq.Options.Any(x => x.Name == "tsize");
    }

    private class StreamThatThrowsExceptionWhenReadingLength : MemoryStream
    {
        public override long Length => throw new NotSupportedException();
    }
}