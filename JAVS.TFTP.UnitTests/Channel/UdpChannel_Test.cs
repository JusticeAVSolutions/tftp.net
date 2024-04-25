using System;
using Tftp.Net.Channel;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Tftp.Net.UnitTests;

public class UdpChannel_Test : IDisposable
{
    private UdpChannel tested;

    public UdpChannel_Test()
    {
        tested = new UdpChannel(new UdpClient(0));
    }

    public void Dispose()
    {
        tested.Dispose();
    }

    [Fact]
    public void SendsRealUdpPackets()
    {
        var remote = OpenRemoteUdpClient();

        tested.Open();
        tested.RemoteEndpoint = remote.Client.LocalEndPoint;
        tested.Send(new Acknowledgement(1));

        AssertBytesReceived(remote, TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void DeniesSendingOnClosedConnections()
    {
        Assert.Throws<InvalidOperationException>(() => tested.Send(new Acknowledgement(1)));
    }

    [Fact]
    public void DeniesSendingWhenNoRemoteAddressIsSet()
    {
        tested.Open();
        Assert.Throws<InvalidOperationException>(() => tested.Send(new Acknowledgement(1)));
    }

    private void AssertBytesReceived(UdpClient remote, TimeSpan timeout)
    {
        double msecs = timeout.TotalMilliseconds;
        while (msecs > 0)
        {
            if (remote.Available > 0)
                return;

            Thread.Sleep(50);
            msecs -= 50;
        }

        Assert.Fail("Remote client did not receive anything within " + timeout.TotalMilliseconds + "ms");
    }

    private UdpClient OpenRemoteUdpClient()
    {
        return new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
    }
}