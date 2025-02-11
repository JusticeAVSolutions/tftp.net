﻿using System.Net;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Transfer.States;

internal class BaseState : ITransferState
{
    public TftpTransfer Context { get; set; }

    public virtual void OnStateEnter()
    {
        //no-op
    }

    public virtual void OnStart()
    {
    }

    public virtual void OnCancel(TftpErrorPacket reason)
    {
    }

    public virtual void OnCommand(ITftpCommand command, EndPoint endpoint)
    {
    }

    public virtual void OnTimer()
    {
        //Ignore timer events
    }
}