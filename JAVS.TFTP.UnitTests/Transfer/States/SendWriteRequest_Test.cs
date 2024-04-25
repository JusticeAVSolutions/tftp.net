using System;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests;

public class SendWriteRequest_Test
{
    private readonly TransferStub _transfer;

    public SendWriteRequest_Test()
    {
        _transfer = new TransferStub(new MemoryStream(new byte[5000]));
        _transfer.SetState(new SendWriteRequest());
    }

    [Fact]
    public void CanCancel()
    {
        _transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.IsType<Closed>(_transfer.State);
        Assert.True(_transfer.CommandWasSent(typeof(Error)));
    }

    [Fact]
    public void SendsWriteRequest()
    {
        var transfer = new TransferStub(new MemoryStream(new byte[5000]));
        transfer.SetState(new SendWriteRequest());
        Assert.True(transfer.CommandWasSent(typeof(WriteRequest)));
    }

    [Fact]
    public void HandlesAcknowledgement()
    {
        _transfer.OnCommand(new Acknowledgement(0));
        Assert.IsType<Sending>(_transfer.State);
    }

    [Fact]
    public void IgnoresWrongAcknowledgement()
    {
        _transfer.OnCommand(new Acknowledgement(5));
        Assert.IsType<SendWriteRequest>(_transfer.State);
    }

    [Fact]
    public void HandlesOptionAcknowledgement()
    {
        _transfer.BlockSize = 999;
        _transfer.OnCommand(new OptionAcknowledgement(new[] { new TransferOption("blksize", "800") }));
        Assert.Equal(800, _transfer.BlockSize);
    }

    [Fact]
    public void HandlesMissingOptionAcknowledgement()
    {
        _transfer.BlockSize = 999;
        _transfer.OnCommand(new Acknowledgement(0));
        Assert.Equal(512, _transfer.BlockSize);
    }

    [Fact]
    public void HandlesError()
    {
        var onErrorWasCalled = false;
        _transfer.OnError += delegate { onErrorWasCalled = true; };

        Assert.False(onErrorWasCalled);
        _transfer.OnCommand(new Error(123, "Test Error"));
        Assert.True(onErrorWasCalled);

        Assert.IsType<Closed>(_transfer.State);
    }

    [Fact]
    public void ResendsRequest()
    {
        var transferWithLowTimeout = new TransferStub(new MemoryStream());
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.SetState(new SendWriteRequest());

        Assert.True(transferWithLowTimeout.CommandWasSent(typeof(WriteRequest)));
        transferWithLowTimeout.SentCommands.Clear();

        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.CommandWasSent(typeof(WriteRequest)));
    }

    [Fact]
    public void TimeoutWhenNoAnswerIsReceivedAndRetryCountIsExceeded()
    {
        var transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.RetryCount = 1;
        transferWithLowTimeout.SetState(new SendWriteRequest());

        transferWithLowTimeout.OnTimer();
        Assert.False(transferWithLowTimeout.HadNetworkTimeout);
        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.HadNetworkTimeout);
    }
}