using System;
using JAVS.TFTP.Channel;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.Transfer;

class RemoteReadTransfer : TftpTransfer
{
    public RemoteReadTransfer(ITransferChannel connection, String filename)
        : base(connection, filename, new StartOutgoingRead())
    {
    }

    public override long ExpectedSize
    {
        get => base.ExpectedSize;
        set => throw new NotSupportedException("You cannot set the expected size of a file that is remotely transferred to this system.");
    }
}