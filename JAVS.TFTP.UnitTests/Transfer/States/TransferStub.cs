using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using JAVS.TFTP.Channel;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.UnitTests.Transfer.States;

internal class TransferStub : TftpTransfer
{
    private class Uninitialized : BaseState
    {
    }

    private ChannelStub Channel => (ChannelStub)_connection;

    public List<ITftpCommand> SentCommands => Channel.SentCommands;

    public bool HadNetworkTimeout { get; set; }

    public TransferStub(MemoryStream stream)
        : base(new ChannelStub(), "dummy.txt", new Uninitialized())
    {
        InputOutputStream = stream;
        HadNetworkTimeout = false;
        OnError += TransferStub_OnError;
    }

    private void TransferStub_OnError(ITftpTransfer transfer, TftpTransferError error)
    {
        if (error is TimeoutError)
            HadNetworkTimeout = true;
    }

    public TransferStub()
        : this(null)
    {
    }

    public ITransferState State => _state;

    public void OnCommand(ITftpCommand command)
    {
        State.OnCommand(command, GetConnection().RemoteEndpoint);
    }

    public bool CommandWasSent(Type commandType)
    {
        return SentCommands.Any(x => x.GetType().IsAssignableFrom(commandType));
    }

    protected override ITransferState DecorateForLogging(ITransferState state)
    {
        return state;
    }

    public void OnTimer()
    {
        _state.OnTimer();
    }

    public override void Dispose()
    {
        //Dont dispose the input/output stream during unit tests
        InputOutputStream = null;

        base.Dispose();
    }
}

internal class ChannelStub : ITransferChannel
{
    public event TftpCommandHandler OnCommandReceived;
    public event TftpChannelErrorHandler OnError;
    public EndPoint RemoteEndpoint { get; set; }
    public readonly List<ITftpCommand> SentCommands = [];

    public ChannelStub()
    {
        RemoteEndpoint = new IPEndPoint(IPAddress.Loopback, 69);
    }

    public void Open()
    {
    }

    public void RaiseCommandReceived(ITftpCommand command, EndPoint endpoint)
    {
        OnCommandReceived?.Invoke(command, endpoint);
    }

    public void RaiseOnError(TftpTransferError error)
    {
        OnError?.Invoke(error);
    }

    public void Send(ITftpCommand command)
    {
        SentCommands.Add(command);
    }

    public void Dispose()
    {
    }
}