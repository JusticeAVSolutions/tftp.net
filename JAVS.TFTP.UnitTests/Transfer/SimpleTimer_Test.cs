using System;
using System.Threading;
using JAVS.TFTP.Transfer;

namespace JAVS.TFTP.UnitTests.Transfer;

public class SimpleTimer_Test
{
    [Fact]
    public void TimesOutWhenTimeoutIsReached()
    {
        var timer = new SimpleTimer(new TimeSpan(500));
        Assert.False(timer.IsTimeout());
        Thread.Sleep(1000);
        Assert.True(timer.IsTimeout());
    }

    [Fact]
    public void RestartingResetsTimeout()
    {
        Thread.Sleep(10);
        var timer = new SimpleTimer(new TimeSpan(500));
        Assert.False(timer.IsTimeout());
        Thread.Sleep(1000);
        Assert.True(timer.IsTimeout());
        timer.Restart();
        Assert.False(timer.IsTimeout());
    }

    [Fact]
    public void ImmediateTimeout()
    {
        var timer = new SimpleTimer(new TimeSpan(0));
        Assert.True(timer.IsTimeout());
    }
}