using JAVS.TFTP.Commands;
using JAVS.TFTP.Trace;

namespace JAVS.TFTP.Transfer.States;

class StateWithNetworkTimeout : BaseState
{
    private SimpleTimer _timer;
    private ITftpCommand _lastCommand;
    private int _retriesUsed;

    public override void OnStateEnter()
    {
        _timer = new SimpleTimer(Context.RetryTimeout);
    }

    public override void OnTimer()
    {
        if (_timer.IsTimeout())
        {
            TftpTrace.Trace("Network timeout.", Context);
            _timer.Restart();

            if (_retriesUsed++ >= Context.RetryCount)
            {
                TftpTransferError error = new TimeoutError(Context.RetryTimeout, Context.RetryCount);
                Context.SetState(new ReceivedError(error));
            }
            else
                HandleTimeout();
        }
    }

    private void HandleTimeout()
    {
        if (_lastCommand != null)
        {
            Context.GetConnection().Send(_lastCommand);
        }
    }

    protected void SendAndRepeat(ITftpCommand command)
    {
        Context.GetConnection().Send(command);
        _lastCommand = command;
        ResetTimeout();
    }

    protected void ResetTimeout()
    {
        _timer.Restart();
        _retriesUsed = 0;
    }
}