﻿using System;
using JAVS.TFTP.Channel;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.Transfer;

internal class RemoteWriteTransfer : TftpTransfer
{
    public RemoteWriteTransfer(ITransferChannel connection, String filename)
        : base(connection, filename, new StartOutgoingWrite())
    {
    }
}