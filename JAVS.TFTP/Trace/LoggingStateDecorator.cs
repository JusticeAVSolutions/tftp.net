using System.Net;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.Trace;

class LoggingStateDecorator : ITransferState
{
    public TftpTransfer Context
    {
        get => _decoratee.Context;
        set => _decoratee.Context = value;
    }

    private readonly ITransferState _decoratee;
    private readonly TftpTransfer _transfer;

    public LoggingStateDecorator(ITransferState decoratee, TftpTransfer transfer)
    {
        _decoratee = decoratee;
        _transfer = transfer;
    }

    public string GetStateName() => $"[{_decoratee.GetType().Name}]";

    public void OnStateEnter()
    {
        TftpTrace.Trace(GetStateName() + " OnStateEnter", _transfer);
        _decoratee.OnStateEnter();
    }

    public void OnStart()
    {
        TftpTrace.Trace(GetStateName() + " OnStart", _transfer);
        _decoratee.OnStart();
    }

    public void OnCancel(TftpErrorPacket reason)
    {
        TftpTrace.Trace(GetStateName() + " OnCancel: " + reason, _transfer);
        _decoratee.OnCancel(reason);
    }

    public void OnCommand(ITftpCommand command, EndPoint endpoint)
    {
        TftpTrace.Trace(GetStateName() + " OnCommand: " + command + " from " + endpoint, _transfer);
        _decoratee.OnCommand(command, endpoint);
    }

    public void OnTimer()
    {
        _decoratee.OnTimer();
    }
}