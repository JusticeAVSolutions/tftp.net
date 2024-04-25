using System;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests;

public class SendWriteRequest_Test
{
    private TransferStub transfer;

    public SendWriteRequest_Test()
    {
        transfer = new TransferStub(new MemoryStream(new byte[5000]));
        transfer.SetState(new SendWriteRequest());
    }

    [Fact]
    public void CanCancel()
    {
        transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.IsType<Closed>(transfer.State);
        Assert.True(transfer.CommandWasSent(typeof(Error)));
    }

    [Fact]
    public void SendsWriteRequest()
    {
        TransferStub transfer = new TransferStub(new MemoryStream(new byte[5000]));
        transfer.SetState(new SendWriteRequest());
        Assert.True(transfer.CommandWasSent(typeof(WriteRequest)));
    }

    [Fact]
    public void HandlesAcknowledgement()
    {
        transfer.OnCommand(new Acknowledgement(0));
        Assert.IsType<Sending>(transfer.State);
    }

    [Fact]
    public void IgnoresWrongAcknowledgement()
    {
        transfer.OnCommand(new Acknowledgement(5));
        Assert.IsType<SendWriteRequest>(transfer.State);
    }

    [Fact]
    public void HandlesOptionAcknowledgement()
    {
        transfer.BlockSize = 999;
        transfer.OnCommand(new OptionAcknowledgement(new TransferOption[] { new TransferOption("blksize", "800") }));
        Assert.Equal(800, transfer.BlockSize);
    }

    [Fact]
    public void HandlesMissingOptionAcknowledgement()
    {
        transfer.BlockSize = 999;
        transfer.OnCommand(new Acknowledgement(0));
        Assert.Equal(512, transfer.BlockSize);
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
    public void ResendsRequest()
    {
        TransferStub transferWithLowTimeout = new TransferStub(new MemoryStream());
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
        TransferStub transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.RetryCount = 1;
        transferWithLowTimeout.SetState(new SendWriteRequest());

        transferWithLowTimeout.OnTimer();
        Assert.False(transferWithLowTimeout.HadNetworkTimeout);
        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.HadNetworkTimeout);
    }
}