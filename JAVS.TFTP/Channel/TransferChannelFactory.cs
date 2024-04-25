using System;
using System.Net;
using System.Net.Sockets;

namespace JAVS.TFTP.Channel;

internal static class TransferChannelFactory
{
    public static ITransferChannel CreateServer(EndPoint localAddress)
    {
        if (localAddress is IPEndPoint point)
            return CreateServerUdp(point);

        throw new NotSupportedException("Unsupported endpoint type.");
    }

    public static ITransferChannel CreateConnection(EndPoint remoteAddress)
    {
        if (remoteAddress is IPEndPoint point)
            return CreateConnectionUdp(point);

        throw new NotSupportedException("Unsupported endpoint type.");
    }

    #region UDP connections

    private static ITransferChannel CreateServerUdp(IPEndPoint localAddress)
    {
        var socket = new UdpClient(localAddress);
        return new UdpChannel(socket);
    }

    private static ITransferChannel CreateConnectionUdp(IPEndPoint remoteAddress)
    {
        var localAddress = new IPEndPoint(IPAddress.Any, 0);
        var channel = new UdpChannel(new UdpClient(localAddress));
        channel.RemoteEndpoint = remoteAddress;
        return channel;
    }

    #endregion
}