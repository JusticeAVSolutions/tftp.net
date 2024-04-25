using System;
using System.IO;
using System.Linq;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.UnitTests.Transfer.States;

public class SendingState_Test
{
    private TransferStub _transfer;

    public SendingState_Test()
    {
        _transfer = new TransferStub(new MemoryStream(new byte[5000]));
        _transfer.SetState(new Sending());
    }

    [Fact]
    public void SendsPacket()
    {
        Assert.True(_transfer.CommandWasSent(typeof(Data)));
    }

    [Fact]
    public void ResendsPacket()
    {
        var transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
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
        var transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
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
        _transfer.SentCommands.Clear();
        Assert.False(_transfer.CommandWasSent(typeof(Data)));

        _transfer.OnCommand(new Acknowledgement(1));
        Assert.True(_transfer.CommandWasSent(typeof(Data)));
    }

    [Fact]
    public void IncreasesBlockCountWithEachAcknowledgement()
    {
        Assert.Equal(1, ((Data)_transfer.SentCommands.Last()).BlockNumber);

        _transfer.OnCommand(new Acknowledgement(1));

        Assert.Equal(2, ((Data)_transfer.SentCommands.Last()).BlockNumber);
    }

    [Fact]
    public void BlockCountWrapsAroundTo0()
    {
        SetupTransferThatWillWrapAroundBlockCount();

        RunTransferUntilBlockCount(65535);
        _transfer.OnCommand(new Acknowledgement(65535));

        Assert.Equal(0, ((Data)_transfer.SentCommands.Last()).BlockNumber);
    }

    [Fact]
    public void BlockCountWrapsAroundTo1()
    {
        SetupTransferThatWillWrapAroundBlockCount();
        _transfer.BlockCounterWrapping = BlockCounterWrapAround.ToOne;

        RunTransferUntilBlockCount(65535);
        _transfer.OnCommand(new Acknowledgement(65535));

        Assert.Equal(1, ((Data)_transfer.SentCommands.Last()).BlockNumber);
    }

    [Fact]
    public void IgnoresWrongAcknowledgment()
    {
        _transfer.SentCommands.Clear();
        Assert.False(_transfer.CommandWasSent(typeof(Data)));

        _transfer.OnCommand(new Acknowledgement(0));
        Assert.False(_transfer.CommandWasSent(typeof(Data)));
    }

    [Fact]
    public void RaisesProgress()
    {
        var onProgressWasCalled = false;
        _transfer.OnProgress += delegate(ITftpTransfer t, TftpTransferProgress progress)
        {
            Assert.Equal(_transfer, t);
            Assert.True(progress.TransferredBytes > 0);
            onProgressWasCalled = true;
        };

        Assert.False(onProgressWasCalled);
        _transfer.OnCommand(new Acknowledgement(1));
        Assert.True(onProgressWasCalled);
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

    private void SetupTransferThatWillWrapAroundBlockCount()
    {
        _transfer = new TransferStub(new MemoryStream(new byte[70000]));
        _transfer.BlockSize = 1;
        _transfer.SetState(new Sending());
    }

    private void RunTransferUntilBlockCount(int targetBlockCount)
    {
        while (((Data)_transfer.SentCommands.Last()).BlockNumber != targetBlockCount)
            _transfer.OnCommand(new Acknowledgement(((Data)_transfer.SentCommands.Last()).BlockNumber));
    }
}