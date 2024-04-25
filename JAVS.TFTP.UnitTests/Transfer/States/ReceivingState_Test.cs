using System;
using System.Linq;
using Tftp.Net.Transfer.States;
using System.IO;

namespace Tftp.Net.UnitTests.Transfer.States;

public class ReceivingState_Test
{
    private MemoryStream ms;
    private TransferStub transfer;

    public ReceivingState_Test()
    {
        ms = new MemoryStream();
        transfer = new TransferStub(ms);
        transfer.SetState(new Receiving());
    }

    [Fact]
    public void ReceivesPacket()
    {
        transfer.OnCommand(new Data(1, new byte[100]));
        Assert.True(transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.Equal(100, ms.Length);
    }

    [Fact]
    public void SendsAcknowledgement()
    {
        transfer.OnCommand(new Data(1, new byte[100]));
        Assert.True(transfer.CommandWasSent(typeof(Acknowledgement)));
    }

    [Fact]
    public void IgnoresWrongPackets()
    {
        transfer.OnCommand(new Data(2, new byte[100]));
        Assert.False(transfer.CommandWasSent(typeof(Acknowledgement)));
        Assert.Equal(0, ms.Length);
    }

    [Fact]
    public void BlockCounterWrapsAroundToZero()
    {
        TransferUntilBlockCounterWrapIsAboutToWrap();

        transfer.OnCommand(new Data(0, new byte[1]));

        Assert.Equal(0, (transfer.SentCommands.Last() as Acknowledgement).BlockNumber);
    }

    [Fact]
    public void BlockCounterWrapsAroundToOne()
    {
        transfer.BlockCounterWrapping = BlockCounterWrapAround.ToOne;
        TransferUntilBlockCounterWrapIsAboutToWrap();

        transfer.OnCommand(new Data(1, new byte[1]));

        Assert.Equal(1, (transfer.SentCommands.Last() as Acknowledgement).BlockNumber);
    }

    private void TransferUntilBlockCounterWrapIsAboutToWrap()
    {
        transfer.BlockSize = 1;
        for (int i = 1; i <= 65535; i++)
            transfer.OnCommand(new Data((ushort)i, new byte[1]));
    }

    [Fact]
    public void RaisesFinished()
    {
        bool onFinishedWasCalled = false;
        transfer.OnFinished += delegate(ITftpTransfer t)
        {
            Assert.Equal(transfer, t);
            onFinishedWasCalled = true;
        };

        Assert.False(onFinishedWasCalled);
        transfer.OnCommand(new Data(1, new byte[100]));
        Assert.True(onFinishedWasCalled);
        Assert.IsType<Closed>(transfer.State);
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
        transfer.OnCommand(new Data(1, new byte[1000]));
        Assert.True(onProgressWasCalled);
        Assert.IsType<Receiving>(transfer.State);
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
    public void TimeoutWhenNoDataIsReceivedAndRetryCountIsExceeded()
    {
        TransferStub transferWithLowTimeout = new TransferStub(new MemoryStream(new byte[5000]));
        transferWithLowTimeout.RetryTimeout = new TimeSpan(0);
        transferWithLowTimeout.RetryCount = 1;
        transferWithLowTimeout.SetState(new Receiving());

        transferWithLowTimeout.OnTimer();
        Assert.False(transferWithLowTimeout.HadNetworkTimeout);
        transferWithLowTimeout.OnTimer();
        Assert.True(transferWithLowTimeout.HadNetworkTimeout);
    }
}