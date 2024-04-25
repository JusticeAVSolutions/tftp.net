using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Channel;

internal class UdpChannel : ITransferChannel
{
    public event TftpCommandHandler OnCommandReceived;
    public event TftpChannelErrorHandler OnError;

    private readonly CommandSerializer _serializer = new();
    private IPEndPoint _endpoint;
    private UdpClient _client;

    public UdpChannel(UdpClient client)
    {
        _client = client;
        _endpoint = null;
    }

    public void Open()
    {
        if (_client == null)
            throw new ObjectDisposedException("UdpChannel");

        _client.BeginReceive(UdpReceivedCallback, null);
    }

    private void UdpReceivedCallback(IAsyncResult result)
    {
        var endpoint = new IPEndPoint(0, 0);
        ITftpCommand command = null;

        try
        {
            byte[] data;

            lock (this)
            {
                if (_client == null)
                    return;

                data = _client.EndReceive(result, ref endpoint);
            }

            command = CommandParser.Parse(data);
        }
        catch (SocketException e)
        {
            //Handle receive error
            RaiseOnError(new NetworkError(e));
        }
        catch (TftpParserException e2)
        {
            //Handle parser error
            RaiseOnError(new NetworkError(e2));
        }

        if (command != null)
        {
            RaiseOnCommand(command, endpoint);
        }

        lock (this)
        {
            if (_client != null)
                _client.BeginReceive(UdpReceivedCallback, null);
        }
    }

    private void RaiseOnCommand(ITftpCommand command, IPEndPoint endpoint)
    {
        OnCommandReceived?.Invoke(command, endpoint);
    }

    private void RaiseOnError(TftpTransferError error)
    {
        OnError?.Invoke(error);
    }

    public void Send(ITftpCommand command)
    {
        if (_client == null)
            throw new ObjectDisposedException("UdpChannel");

        if (_endpoint == null)
            throw new InvalidOperationException("RemoteEndpoint needs to be set before you can send TFTP commands.");

        using (var stream = new MemoryStream())
        {
            _serializer.Serialize(command, stream);
            byte[] data = stream.GetBuffer();
            _client.Send(data, (int)stream.Length, _endpoint);
        }
    }

    public void Dispose()
    {
        lock (this)
        {
            if (_client == null)
                return;

            _client.Close();
            _client = null;
        }
    }

    public EndPoint RemoteEndpoint
    {
        get => _endpoint;

        set
        {
            if (value is not IPEndPoint point)
                throw new NotSupportedException("UdpChannel can only connect to IPEndPoints.");

            if (_client == null)
                throw new ObjectDisposedException("UdpChannel");

            _endpoint = point;
        }
    }
}