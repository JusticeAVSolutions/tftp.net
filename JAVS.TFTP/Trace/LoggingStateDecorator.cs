using System.Net;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.Trace;

internal class LoggingStateDecorator : ITransferState
{
    public TftpTransfer Context
    {
        get => _innerState.Context;
        set => _innerState.Context = value;
    }

    private readonly ITransferState _innerState;
    private readonly TftpTransfer _transfer;

    public LoggingStateDecorator(ITransferState innerState, TftpTransfer transfer)
    {
        _innerState = innerState;
        _transfer = transfer;
    }

    public string GetStateName() => $"[{_innerState.GetType().Name}]";

    public void OnStateEnter()
    {
        TftpTrace.Trace(GetStateName() + " OnStateEnter", _transfer);
        _innerState.OnStateEnter();
    }

    public void OnStart()
    {
        TftpTrace.Trace(GetStateName() + " OnStart", _transfer);
        _innerState.OnStart();
    }

    public void OnCancel(TftpErrorPacket reason)
    {
        TftpTrace.Trace(GetStateName() + " OnCancel: " + reason, _transfer);
        _innerState.OnCancel(reason);
    }

    public void OnCommand(ITftpCommand command, EndPoint endpoint)
    {
        TftpTrace.Trace(GetStateName() + " OnCommand: " + command + " from " + endpoint, _transfer);
        _innerState.OnCommand(command, endpoint);
    }

    public void OnTimer()
    {
        _innerState.OnTimer();
    }
}