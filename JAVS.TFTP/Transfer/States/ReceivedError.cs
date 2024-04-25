using JAVS.TFTP.Commands;
using JAVS.TFTP.Trace;

namespace JAVS.TFTP.Transfer.States;

internal class ReceivedError : BaseState
{
    private readonly TftpTransferError _error;

    public ReceivedError(Error error)
        : this(new TftpErrorPacket(error.ErrorCode, GetNonEmptyErrorMessage(error)))
    {
    }

    private static string GetNonEmptyErrorMessage(Error error) =>
        string.IsNullOrEmpty(error.Message) ? "(No error message provided)" : error.Message;

    public ReceivedError(TftpTransferError error)
    {
        _error = error;
    }

    public override void OnStateEnter()
    {
        TftpTrace.Trace("Received error: " + _error, Context);
        Context.RaiseOnError(_error);
        Context.SetState(new Closed());
    }
}