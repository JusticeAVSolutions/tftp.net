using System;
using System.Threading;
using Tftp.Net.Transfer;

namespace Tftp.Net.UnitTests.Transfer;

public class SimpleTimer_Test
{
    [Fact]
    public void TimesOutWhenTimeoutIsReached()
    {
        SimpleTimer timer = new SimpleTimer(new TimeSpan(100));
        Assert.False(timer.IsTimeout());
        Thread.Sleep(200);
        Assert.True(timer.IsTimeout());
    }

    [Fact]
    public void RestartingResetsTimeout()
    {
        SimpleTimer timer = new SimpleTimer(new TimeSpan(100));
        Assert.False(timer.IsTimeout());
        Thread.Sleep(200);
        Assert.True(timer.IsTimeout());
        timer.Restart();
        Assert.False(timer.IsTimeout());
    }

    [Fact]
    public void ImmediateTimeout()
    {
        SimpleTimer timer = new SimpleTimer(new TimeSpan(0));
        Assert.True(timer.IsTimeout());
    }
}