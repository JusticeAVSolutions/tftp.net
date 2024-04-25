using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.UnitTests.Transfer.States;

public class CancelledByUserState_Test
{
    private readonly TransferStub _transfer;

    public CancelledByUserState_Test()
    {
        _transfer = new TransferStub();
        _transfer.SetState(new CancelledByUser(TftpErrorPacket.IllegalOperation));
    }

    [Fact]
    public void SendsErrorToClient()
    {
        Assert.True(_transfer.CommandWasSent(typeof(Error)));
    }

    [Fact]
    public void TransitionsToClosedState()
    {
        Assert.IsType<Closed>(_transfer.State);
    }
}