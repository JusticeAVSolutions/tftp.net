using System;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests;

public class SendReadRequestState_Test
{
    private MemoryStream ms;
    private TransferStub transfer;

    public SendReadRequestState_Test()
    {
        ms = new MemoryStream();
        transfer = new TransferStub(ms);
        transfer.SetState(new SendReadRequest());
    }

    [Fact]
    public void CanCancel()
    {
        transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.True(transfer.CommandWasSent(typeof(Error)));
        Assert.IsType<Closed>(transfer.State);
    }

    [Fact]
    public void HandlesError()
    {
        bool onErrorWasCalled = false;
        transfer.OnError += delegate(ITftpTransfer t, TftpTransferError error) { onErrorWasCalled = true; };

        Assert.False(onErrorWasCalled);
        transfer.OnCommand(new Error(123, "Test Error"));
        Assert.True(onErrorWasCalled);

        Assert.IsType<Closed>(transfer.State);
    }

    [Fact]
    public void HandlesData()
    {
        transfer.OnCommand(new Data(1, new byte[10]));
        Assert.True(transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.IsType<Closed>(transfer.State);
        Assert.Equal(10, ms.Length);
    }

    [Fact]
    public void HandlesOptionAcknowledgement()
    {
        transfer.BlockSize = 999;
        transfer.OnCommand(new OptionAcknowledgement(new TransferOption[] { new TransferOption("blksize", "999") }));
        Assert.True(transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.Equal(999, transfer.BlockSize);
    }

    [Fact]
    public void HandlesMissingOptionAcknowledgement()
    {
        transfer.BlockSize = 999;
        transfer.OnCommand(new Data(1, new byte[10]));
        Assert.Equal(512, transfer.BlockSize);
    }

    [Fact]
    public void SendsRequest()
    {
        Assert.True(transfer.CommandWasSent(typeof(ReadRequest)));
    }

    [Fact]
    public void ResendsRequest()
    {
        TransferStub transferWithLowTimeout = new TransferStub(new MemoryStream());
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
        TransferStub transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.RetryCount = 1;
        transferWithLowTimeout.SetState(new SendReadRequest());

        transferWithLowTimeout.OnTimer();
        Assert.False(transferWithLowTimeout.HadNetworkTimeout);
        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.HadNetworkTimeout);
    }
}