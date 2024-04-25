using Tftp.Net.Transfer.States;

namespace Tftp.Net.UnitTests;

public class ClosedState_Test
{
    private readonly TransferStub _transfer;

    public ClosedState_Test()
    {
        _transfer = new TransferStub();
        _transfer.SetState(new Closed());
    }

    [Fact]
    public void CanNotCancel()
    {
        _transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.IsType<Closed>(_transfer.State);
    }

    [Fact]
    public void IgnoresCommands()
    {
        _transfer.OnCommand(new Error(10, "Test"));
        Assert.IsType<Closed>(_transfer.State);
    }
}