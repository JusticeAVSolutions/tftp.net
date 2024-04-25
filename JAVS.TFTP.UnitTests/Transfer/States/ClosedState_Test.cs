using Tftp.Net.Transfer.States;

namespace Tftp.Net.UnitTests;

public class ClosedState_Test
{
    private TransferStub transfer;

    public ClosedState_Test()
    {
        transfer = new TransferStub();
        transfer.SetState(new Closed());
    }

    [Fact]
    public void CanNotCancel()
    {
        transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.IsType<Closed>(transfer.State);
    }

    [Fact]
    public void IgnoresCommands()
    {
        transfer.OnCommand(new Error(10, "Test"));
        Assert.IsType<Closed>(transfer.State);
    }
}