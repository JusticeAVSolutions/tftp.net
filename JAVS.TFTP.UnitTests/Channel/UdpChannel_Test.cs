using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JAVS.TFTP.Channel;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.UnitTests.Channel;

public sealed class UdpChannel_Test : IDisposable
{
    private readonly UdpChannel _tested = new(new UdpClient(0));

    public void Dispose() => _tested.Dispose();

    [Fact]
    public void SendsRealUdpPackets()
    {
        var remote = OpenRemoteUdpClient();

        _tested.Open();
        _tested.RemoteEndpoint = remote.Client.LocalEndPoint;
        _tested.Send(new Acknowledgement(1));

        AssertBytesReceived(remote, TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void DeniesSendingOnClosedConnections()
    {
        Assert.Throws<InvalidOperationException>(() => _tested.Send(new Acknowledgement(1)));
    }

    [Fact]
    public void DeniesSendingWhenNoRemoteAddressIsSet()
    {
        _tested.Open();
        Assert.Throws<InvalidOperationException>(() => _tested.Send(new Acknowledgement(1)));
    }

    private static void AssertBytesReceived(UdpClient remote, TimeSpan timeout)
    {
        var msecs = timeout.TotalMilliseconds;
        while (msecs > 0)
        {
            if (remote.Available > 0)
                return;

            Thread.Sleep(50);
            msecs -= 50;
        }

        Assert.Fail("Remote client did not receive anything within " + timeout.TotalMilliseconds + "ms");
    }

    private static UdpClient OpenRemoteUdpClient() => new(new IPEndPoint(IPAddress.Loopback, 0));
}