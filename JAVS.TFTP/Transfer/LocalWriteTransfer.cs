using System;
using System.Collections.Generic;
using JAVS.TFTP.Channel;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.Transfer;

class LocalWriteTransfer : TftpTransfer
{
    public LocalWriteTransfer(ITransferChannel connection, string filename, IEnumerable<TransferOption> options)
        : base(connection, filename, new StartIncomingWrite(options))
    {
    }

    public override TftpTransferMode TransferMode
    {
        get => base.TransferMode;
        set => throw new NotSupportedException("Cannot change the transfer mode for incoming transfers. The transfer mode is determined by the client.");
    }

    public override int BlockSize
    {
        get => base.BlockSize;
        set => throw new NotSupportedException("For incoming transfers, the blocksize is determined by the client.");
    }

    public override TimeSpan RetryTimeout
    {
        get => base.RetryTimeout;
        set => throw new NotSupportedException("For incoming transfers, the retry timeout is determined by the client.");
    }

    public override long ExpectedSize
    {
        get => base.ExpectedSize;
        set => throw new NotSupportedException("You cannot set the expected size of a file that is remotely transferred to this system.");
    }
}