﻿using System;
using System.Net;
using JAVS.TFTP.Channel;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer;

namespace JAVS.TFTP;

public delegate void TftpServerEventHandler(ITftpTransfer transfer, EndPoint client);

public delegate void TftpServerErrorHandler(TftpTransferError error);

/// <summary>
/// A simple TFTP server class. <code>Dispose()</code> the server to close the socket that it listens on.
/// </summary>
public class TftpServer : IDisposable
{
    public const int DEFAULT_SERVER_PORT = 69;

    /// <summary>
    /// Fired when the server receives a new read request.
    /// </summary>
    public event TftpServerEventHandler OnReadRequest;

    /// <summary>
    /// Fired when the server receives a new write request.
    /// </summary>
    public event TftpServerEventHandler OnWriteRequest;

    /// <summary>
    /// Fired when the server encounters an error (for example, a non-parseable request)
    /// </summary>
    public event TftpServerErrorHandler OnError;

    /// <summary>
    /// Server port that we're listening on.
    /// </summary>
    private readonly ITransferChannel _serverSocket;

    public TftpServer(IPEndPoint localAddress)
    {
        if (localAddress == null)
            throw new ArgumentNullException(nameof(localAddress));

        _serverSocket = TransferChannelFactory.CreateServer(localAddress);
        _serverSocket.OnCommandReceived += ServerSocket_OnCommandReceived;
        _serverSocket.OnError += ServerSocket_OnError;
    }

    public TftpServer(IPAddress localAddress)
        : this(localAddress, DEFAULT_SERVER_PORT)
    {
    }

    public TftpServer(IPAddress localAddress, int port)
        : this(new IPEndPoint(localAddress, port))
    {
    }

    public TftpServer(int port)
        : this(new IPEndPoint(IPAddress.Any, port))
    {
    }

    public TftpServer()
        : this(DEFAULT_SERVER_PORT)
    {
    }


    /// <summary>
    /// Start accepting incoming connections.
    /// </summary>
    public void Start()
    {
        _serverSocket.Open();
    }

    private void ServerSocket_OnError(TftpTransferError error)
    {
        RaiseOnError(error);
    }

    private void ServerSocket_OnCommandReceived(ITftpCommand command, EndPoint endpoint)
    {
        //Ignore all other commands
        if (command is not ReadOrWriteRequest request)
            return;

        //Open a connection to the client
        var channel = TransferChannelFactory.CreateConnection(endpoint);

        //Create a wrapper for the transfer request
        ITftpTransfer transfer = request is ReadRequest
            ? new LocalReadTransfer(channel, request.Filename, request.Options)
            : new LocalWriteTransfer(channel, request.Filename, request.Options);

        if (request is ReadRequest)
            RaiseOnReadRequest(transfer, endpoint);
        else if (request is WriteRequest)
            RaiseOnWriteRequest(transfer, endpoint);
        else
            throw new Exception("Unexpected tftp transfer request: " + request);
    }

    #region IDisposable

    public void Dispose()
    {
        _serverSocket.Dispose();
    }

    #endregion

    private void RaiseOnError(TftpTransferError error)
    {
        OnError?.Invoke(error);
    }

    private void RaiseOnWriteRequest(ITftpTransfer transfer, EndPoint client)
    {
        if (OnWriteRequest != null)
        {
            OnWriteRequest(transfer, client);
        }
        else
        {
            transfer.Cancel(new TftpErrorPacket(0, "Server did not provide a OnWriteRequest handler."));
        }
    }

    private void RaiseOnReadRequest(ITftpTransfer transfer, EndPoint client)
    {
        if (OnReadRequest != null)
        {
            OnReadRequest(transfer, client);
        }
        else
        {
            transfer.Cancel(new TftpErrorPacket(0, "Server did not provide a OnReadRequest handler."));
        }
    }
}