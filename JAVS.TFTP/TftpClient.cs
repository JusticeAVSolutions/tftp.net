﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using JAVS.TFTP.Channel;
using JAVS.TFTP.Transfer;

namespace JAVS.TFTP;

/// <summary>
/// A TFTP client that can connect to a TFTP server.
/// </summary>
public class TftpClient
{
    private const int DEFAULT_SERVER_PORT = 69;
    private readonly IPEndPoint _remoteAddress;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="remoteAddress">Address of the server that you would like to connect to.</param>
    public TftpClient(IPEndPoint remoteAddress)
    {
        _remoteAddress = remoteAddress;
    }

    /// <summary>
    /// Connects to a server
    /// </summary>
    /// <param name="ip">Address of the server that you want connect to.</param>
    /// <param name="port">Port on the server that you want to connect to (default: 69)</param>
    public TftpClient(IPAddress ip, int port)
        : this(new IPEndPoint(ip, port))
    {
    }

    /// <summary>
    /// Connects to a server on port 69.
    /// </summary>
    /// <param name="ip">Address of the server that you want to connect to.</param>
    public TftpClient(IPAddress ip)
        : this(new IPEndPoint(ip, DEFAULT_SERVER_PORT))
    {
    }

    /// <summary>
    /// Connect to a server by hostname.
    /// </summary>
    /// <param name="host">Hostname or ip to connect to</param>
    public TftpClient(string host)
        : this(host, DEFAULT_SERVER_PORT)
    {
    }

    /// <summary>
    /// Connect to a server by hostname and port .
    /// </summary>
    /// <param name="host">Hostname or ip to connect to</param>
    /// <param name="port">Port to connect to</param>
    public TftpClient(string host, int port)
    {
        var ip = Dns.GetHostAddresses(host).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

        if (ip == null)
            throw new ArgumentException($"Could not convert '{host}' to an IPv4 address.", nameof(host));

        _remoteAddress = new IPEndPoint(ip, port);
    }

    /// <summary>
    /// GET a file from the server.
    /// You have to call Start() on the returned ITftpTransfer to start the transfer.
    /// </summary>
    public ITftpTransfer Download(string filename)
    {
        var channel = TransferChannelFactory.CreateConnection(_remoteAddress);
        return new RemoteReadTransfer(channel, filename);
    }

    /// <summary>
    /// PUT a file from the server.
    /// You have to call Start() on the returned ITftpTransfer to start the transfer.
    /// </summary>
    public ITftpTransfer Upload(string filename)
    {
        var channel = TransferChannelFactory.CreateConnection(_remoteAddress);
        return new RemoteWriteTransfer(channel, filename);
    }
}