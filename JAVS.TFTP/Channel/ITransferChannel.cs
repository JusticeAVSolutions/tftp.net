using System;
using System.Net;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Channel;

internal delegate void TftpCommandHandler(ITftpCommand command, EndPoint endpoint);

internal delegate void TftpChannelErrorHandler(TftpTransferError error);

internal interface ITransferChannel : IDisposable
{
    event TftpCommandHandler OnCommandReceived;
    event TftpChannelErrorHandler OnError;

    EndPoint RemoteEndpoint { get; set; }

    void Open();
    void Send(ITftpCommand command);
}