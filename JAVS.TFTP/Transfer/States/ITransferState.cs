using System.Net;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Transfer.States;

internal interface ITransferState
{
    TftpTransfer Context { get; set; }

    //Called by TftpTransfer
    void OnStateEnter();

    //Called if the user calls TftpTransfer.Start() or TftpTransfer.Cancel()
    void OnStart();
    void OnCancel(TftpErrorPacket reason);

    //Called regularly by the context
    void OnTimer();

    //Called when a command is received
    void OnCommand(ITftpCommand command, EndPoint endpoint);
}