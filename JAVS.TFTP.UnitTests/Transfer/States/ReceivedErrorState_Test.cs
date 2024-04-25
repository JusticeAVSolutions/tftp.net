using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.UnitTests.Transfer.States;

public class ReceivedErrorState_Test
{
    private readonly TransferStub _transfer;

    public ReceivedErrorState_Test()
    {
        _transfer = new TransferStub();
        _transfer.SetState(new ReceivedError(new TftpErrorPacket(123, "Error")));
    }

    [Fact]
    public void CallsOnError()
    {
        var onErrorWasCalled = false;
        var transfer = new TransferStub();
        transfer.OnError += delegate(ITftpTransfer t, TftpTransferError error)
        {
            onErrorWasCalled = true;
            Assert.Equal(transfer, t);

            Assert.IsType<TftpErrorPacket>(error);

            Assert.Equal(123, ((TftpErrorPacket)error).ErrorCode);
            Assert.Equal("My Error", ((TftpErrorPacket)error).ErrorMessage);
        };

        Assert.False(onErrorWasCalled);
        transfer.SetState(new ReceivedError(new TftpErrorPacket(123, "My Error")));
        Assert.True(onErrorWasCalled);
    }

    [Fact]
    public void TransitionsToClosed()
    {
        Assert.IsType<Closed>(_transfer.State);
    }
}