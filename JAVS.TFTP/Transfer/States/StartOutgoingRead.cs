﻿namespace JAVS.TFTP.Transfer.States;

internal class StartOutgoingRead : BaseState
{
    public override void OnStart()
    {
        Context.SetState(new SendReadRequest());
    }

    public override void OnCancel(TftpErrorPacket reason)
    {
        Context.SetState(new Closed());
    }
}