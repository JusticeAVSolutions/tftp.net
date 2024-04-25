using System;
using System.Net;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Channel;

delegate void TftpCommandHandler(ITftpCommand command, EndPoint endpoint);

delegate void TftpChannelErrorHandler(TftpTransferError error);

interface ITransferChannel : IDisposable
{
    event TftpCommandHandler OnCommandReceived;
    event TftpChannelErrorHandler OnError;

    EndPoint RemoteEndpoint { get; set; }

    void Open();
    void Send(ITftpCommand command);
}