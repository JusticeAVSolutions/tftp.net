using System;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests;

public class SendReadRequestState_Test
{
    private readonly MemoryStream _ms;
    private readonly TransferStub _transfer;

    public SendReadRequestState_Test()
    {
        _ms = new MemoryStream();
        _transfer = new TransferStub(_ms);
        _transfer.SetState(new SendReadRequest());
    }

    [Fact]
    public void CanCancel()
    {
        _transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.True(_transfer.CommandWasSent(typeof(Error)));
        Assert.IsType<Closed>(_transfer.State);
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
    public void HandlesData()
    {
        _transfer.OnCommand(new Data(1, new byte[10]));
        Assert.True(_transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.IsType<Closed>(_transfer.State);
        Assert.Equal(10, _ms.Length);
    }

    [Fact]
    public void HandlesOptionAcknowledgement()
    {
        _transfer.BlockSize = 999;
        _transfer.OnCommand(new OptionAcknowledgement(new[] { new TransferOption("blksize", "999") }));
        Assert.True(_transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.Equal(999, _transfer.BlockSize);
    }

    [Fact]
    public void HandlesMissingOptionAcknowledgement()
    {
        _transfer.BlockSize = 999;
        _transfer.OnCommand(new Data(1, new byte[10]));
        Assert.Equal(512, _transfer.BlockSize);
    }

    [Fact]
    public void SendsRequest()
    {
        Assert.True(_transfer.CommandWasSent(typeof(ReadRequest)));
    }

    [Fact]
    public void ResendsRequest()
    {
        var transferWithLowTimeout = new TransferStub(new MemoryStream());
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.SetState(new SendReadRequest());

        Assert.True(transferWithLowTimeout.CommandWasSent(typeof(ReadRequest)));
        transferWithLowTimeout.SentCommands.Clear();

        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.CommandWasSent(typeof(ReadRequest)));
    }

    [Fact]
    public void TimeoutWhenNoAnswerIsReceivedAndRetryCountIsExceeded()
    {
        var transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.RetryCount = 1;
        transferWithLowTimeout.SetState(new SendReadRequest());

        transferWithLowTimeout.OnTimer();
        Assert.False(transferWithLowTimeout.HadNetworkTimeout);
        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.HadNetworkTimeout);
    }
}