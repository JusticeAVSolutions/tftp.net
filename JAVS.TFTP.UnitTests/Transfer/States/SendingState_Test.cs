using System;
using System.Linq;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests.Transfer.States;

public class SendingState_Test
{
    private TransferStub transfer;

    public SendingState_Test()
    {
        transfer = new TransferStub(new MemoryStream(new byte[5000]));
        transfer.SetState(new Sending());
    }

    [Fact]
    public void SendsPacket()
    {
        Assert.True(transfer.CommandWasSent(typeof(Data)));
    }

    [Fact]
    public void ResendsPacket()
    {
        TransferStub transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.SetState(new Sending());

        Assert.True(transferWithLowTimeout.CommandWasSent(typeof(Data)));
        transferWithLowTimeout.SentCommands.Clear();

        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.CommandWasSent(typeof(Data)));
    }

    [Fact]
    public void TimeoutWhenNoAnswerIsReceivedAndRetryCountIsExceeded()
    {
        TransferStub transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.RetryCount = 1;
        transferWithLowTimeout.SetState(new Sending());

        transferWithLowTimeout.OnTimer();
        Assert.False(transferWithLowTimeout.HadNetworkTimeout);
        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.HadNetworkTimeout);
    }

    [Fact]
    public void HandlesAcknowledgment()
    {
        transfer.SentCommands.Clear();
        Assert.False(transfer.CommandWasSent(typeof(Data)));

        transfer.OnCommand(new Acknowledgement(1));
        Assert.True(transfer.CommandWasSent(typeof(Data)));
    }

    [Fact]
    public void IncreasesBlockCountWithEachAcknowledgement()
    {
        Assert.Equal(1, (transfer.SentCommands.Last() as Data).BlockNumber);

        transfer.OnCommand(new Acknowledgement(1));

        Assert.Equal(2, (transfer.SentCommands.Last() as Data).BlockNumber);
    }

    [Fact]
    public void BlockCountWrapsAroundTo0()
    {
        SetupTransferThatWillWrapAroundBlockCount();

        RunTransferUntilBlockCount(65535);
        transfer.OnCommand(new Acknowledgement(65535));

        Assert.Equal(0, (transfer.SentCommands.Last() as Data).BlockNumber);
    }

    [Fact]
    public void BlockCountWrapsAroundTo1()
    {
        SetupTransferThatWillWrapAroundBlockCount();
        transfer.BlockCounterWrapping = BlockCounterWrapAround.ToOne;

        RunTransferUntilBlockCount(65535);
        transfer.OnCommand(new Acknowledgement(65535));

        Assert.Equal(1, (transfer.SentCommands.Last() as Data).BlockNumber);
    }

    [Fact]
    public void IgnoresWrongAcknowledgment()
    {
        transfer.SentCommands.Clear();
        Assert.False(transfer.CommandWasSent(typeof(Data)));

        transfer.OnCommand(new Acknowledgement(0));
        Assert.False(transfer.CommandWasSent(typeof(Data)));
    }

    [Fact]
    public void RaisesProgress()
    {
        bool onProgressWasCalled = false;
        transfer.OnProgress += delegate(ITftpTransfer t, TftpTransferProgress progress)
        {
            Assert.Equal(transfer, t);
            Assert.True(progress.TransferredBytes > 0);
            onProgressWasCalled = true;
        };

        Assert.False(onProgressWasCalled);
        transfer.OnCommand(new Acknowledgement(1));
        Assert.True(onProgressWasCalled);
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

    private void SetupTransferThatWillWrapAroundBlockCount()
    {
        transfer = new TransferStub(new MemoryStream(new byte[70000]));
        transfer.BlockSize = 1;
        transfer.SetState(new Sending());
    }

    private void RunTransferUntilBlockCount(int targetBlockCount)
    {
        while ((transfer.SentCommands.Last() as Data).BlockNumber != targetBlockCount)
            transfer.OnCommand(new Acknowledgement((transfer.SentCommands.Last() as Data).BlockNumber));
    }
}