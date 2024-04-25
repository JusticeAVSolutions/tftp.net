using Tftp.Net.Transfer.States;

namespace Tftp.Net.UnitTests;

public class CancelledByUserState_Test
{
    private TransferStub transfer;

    public CancelledByUserState_Test()
    {
        transfer = new TransferStub();
        transfer.SetState(new CancelledByUser(TftpErrorPacket.IllegalOperation));
    }

    [Fact]
    public void SendsErrorToClient()
    {
        Assert.True(transfer.CommandWasSent(typeof(Error)));
    }

    [Fact]
    public void TransitionsToClosedState()
    {
        Assert.IsType<Closed>(transfer.State);
    }
}