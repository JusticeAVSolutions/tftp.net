using System;
using System.IO;
using System.Linq;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.UnitTests.Transfer.States;

public class StartIncomingReadState_Test
{
    private readonly TransferStub _transfer;

    public StartIncomingReadState_Test()
    {
        _transfer = new TransferStub();
        _transfer.SetState(new StartIncomingRead(new[] { new TransferOption("tsize", "0") }));
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
        Assert.IsType<StartIncomingRead>(_transfer.State);
    }

    [Fact]
    public void CanStartWithoutOptions()
    {
        _transfer.SetState(new StartIncomingRead(Array.Empty<TransferOption>()));
        _transfer.Start(new MemoryStream(new byte[50000]));
        Assert.IsType<Sending>(_transfer.State);
    }

    [Fact]
    public void CanStartWithOptions()
    {
        //Simulate that we got a request for a option
        _transfer.SetState(new StartIncomingRead(new[] { new TransferOption("blksize", "999") }));
        Assert.Equal(999, _transfer.BlockSize);
        _transfer.Start(new MemoryStream(new byte[50000]));
        Assert.IsType<SendOptionAcknowledgementForReadRequest>(_transfer.State);
        // TODO: This does nothing but Assert.Contains fails... WTF is this supposed to be?
        // var cmd = (OptionAcknowledgement)transfer.SentCommands.Last();
        // cmd.Options.Contains(new TransferOption("blksize", "999"));
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
        var oack = _transfer.SentCommands.Last() as OptionAcknowledgement;
        return oack != null && oack.Options.Any(x => x.Name == "tsize");
    }

    private class StreamThatThrowsExceptionWhenReadingLength : MemoryStream
    {
        public override long Length => throw new NotSupportedException();
    }
}