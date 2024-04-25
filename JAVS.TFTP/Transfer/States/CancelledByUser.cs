using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Transfer.States;

internal class CancelledByUser : BaseState
{
    private readonly TftpErrorPacket _reason;

    public CancelledByUser(TftpErrorPacket reason)
    {
        _reason = reason;
    }

    public override void OnStateEnter()
    {
        var command = new Error(_reason.ErrorCode, _reason.ErrorMessage);
        Context.GetConnection().Send(command);
        Context.SetState(new Closed());
    }
}