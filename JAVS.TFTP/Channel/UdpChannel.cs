using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Channel;

class UdpChannel : ITransferChannel
{
    public event TftpCommandHandler OnCommandReceived;
    public event TftpChannelErrorHandler OnError;

    private IPEndPoint endpoint;
    private UdpClient client;
    private readonly CommandSerializer serializer = new();
    private readonly CommandParser parser = new();

    public UdpChannel(UdpClient client)
    {
        this.client = client;
        endpoint = null;
    }

    public void Open()
    {
        if (client == null)
            throw new ObjectDisposedException("UdpChannel");

        client.BeginReceive(UdpReceivedCallback, null);
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
                if (client == null)
                    return;

                data = client.EndReceive(result, ref endpoint);
            }

            command = parser.Parse(data);
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
            if (client != null)
                client.BeginReceive(UdpReceivedCallback, null);
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
        if (client == null)
            throw new ObjectDisposedException("UdpChannel");

        if (endpoint == null)
            throw new InvalidOperationException("RemoteEndpoint needs to be set before you can send TFTP commands.");

        using (var stream = new MemoryStream())
        {
            serializer.Serialize(command, stream);
            byte[] data = stream.GetBuffer();
            client.Send(data, (int)stream.Length, endpoint);
        }
    }

    public void Dispose()
    {
        lock (this)
        {
            if (client == null)
                return;

            client.Close();
            client = null;
        }
    }

    public EndPoint RemoteEndpoint
    {
        get => endpoint;

        set
        {
            if (value is not IPEndPoint point)
                throw new NotSupportedException("UdpChannel can only connect to IPEndPoints.");

            if (client == null)
                throw new ObjectDisposedException("UdpChannel");

            endpoint = point;
        }
    }
}