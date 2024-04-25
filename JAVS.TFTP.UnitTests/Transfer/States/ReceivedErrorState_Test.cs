using Tftp.Net.Transfer.States;

namespace Tftp.Net.UnitTests;

public class ReceivedErrorState_Test
{
    private TransferStub transfer;

    public ReceivedErrorState_Test()
    {
        transfer = new TransferStub();
        transfer.SetState(new ReceivedError(new TftpErrorPacket(123, "Error")));
    }

    [Fact]
    public void CallsOnError()
    {
        bool OnErrorWasCalled = false;
        TransferStub transfer = new TransferStub();
        transfer.OnError += delegate(ITftpTransfer t, TftpTransferError error)
        {
            OnErrorWasCalled = true;
            Assert.Equal(transfer, t);

            Assert.IsType<TftpErrorPacket>(error);

            Assert.Equal(123, ((TftpErrorPacket)error).ErrorCode);
            Assert.Equal("My Error", ((TftpErrorPacket)error).ErrorMessage);
        };

        Assert.False(OnErrorWasCalled);
        transfer.SetState(new ReceivedError(new TftpErrorPacket(123, "My Error")));
        Assert.True(OnErrorWasCalled);
    }

    [Fact]
    public void TransitionsToClosed()
    {
        Assert.IsType<Closed>(transfer.State);
    }
}