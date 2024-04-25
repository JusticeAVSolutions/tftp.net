using System;
using System.Linq;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests.Transfer.States;

public class ReceivingState_Test
{
    private readonly MemoryStream _ms;
    private readonly TransferStub _transfer;

    public ReceivingState_Test()
    {
        _ms = new MemoryStream();
        _transfer = new TransferStub(_ms);
        _transfer.SetState(new Receiving());
    }

    [Fact]
    public void ReceivesPacket()
    {
        _transfer.OnCommand(new Data(1, new byte[100]));
        Assert.True(_transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.Equal(100, _ms.Length);
    }

    [Fact]
    public void SendsAcknowledgement()
    {
        _transfer.OnCommand(new Data(1, new byte[100]));
        Assert.True(_transfer.CommandWasSent(typeof(Acknowledgement)));
    }

    [Fact]
    public void IgnoresWrongPackets()
    {
        _transfer.OnCommand(new Data(2, new byte[100]));
        Assert.False(_transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.Equal(0, _ms.Length);
    }

    [Fact]
    public void BlockCounterWrapsAroundToZero()
    {
        TransferUntilBlockCounterWrapIsAboutToWrap();

        _transfer.OnCommand(new Data(0, new byte[1]));

        Assert.Equal(0, ((Acknowledgement)_transfer.SentCommands.Last()).BlockNumber);
    }

    [Fact]
    public void BlockCounterWrapsAroundToOne()
    {
        _transfer.BlockCounterWrapping = BlockCounterWrapAround.ToOne;
        TransferUntilBlockCounterWrapIsAboutToWrap();

        _transfer.OnCommand(new Data(1, new byte[1]));

        Assert.Equal(1, ((Acknowledgement)_transfer.SentCommands.Last()).BlockNumber);
    }

    private void TransferUntilBlockCounterWrapIsAboutToWrap()
    {
        _transfer.BlockSize = 1;
        for (int i = 1; i <= 65535; i++)
            _transfer.OnCommand(new Data((ushort)i, new byte[1]));
    }

    [Fact]
    public void RaisesFinished()
    {
        var onFinishedWasCalled = false;
        _transfer.OnFinished += delegate(ITftpTransfer t)
        {
            Assert.Equal(_transfer, t);
            onFinishedWasCalled = true;
        };

        Assert.False(onFinishedWasCalled);
        _transfer.OnCommand(new Data(1, new byte[100]));
        Assert.True(onFinishedWasCalled);
        Assert.IsType<Closed>(_transfer.State);
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
        _transfer.OnCommand(new Data(1, new byte[1000]));
        Assert.True(onProgressWasCalled);
        Assert.IsType<Receiving>(_transfer.State);
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
    public void TimeoutWhenNoDataIsReceivedAndRetryCountIsExceeded()
    {
        var transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.RetryCount = 1;
        transferWithLowTimeout.SetState(new Receiving());

        transferWithLowTimeout.OnTimer();
        Assert.False(transferWithLowTimeout.HadNetworkTimeout);
        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.HadNetworkTimeout);
    }
}