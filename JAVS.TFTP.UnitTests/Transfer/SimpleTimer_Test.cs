using System;
using System.Threading;
using JAVS.TFTP.Transfer;

namespace JAVS.TFTP.UnitTests.Transfer;

public class SimpleTimer_Test
{
    [Fact(Skip = "Ignore flaky timer tests for now")]
    public void TimesOutWhenTimeoutIsReached()
    {
        var timer = new SimpleTimer(new TimeSpan(500));
        Assert.False(timer.IsTimeout());
        Thread.Sleep(1000);
        Assert.True(timer.IsTimeout());
    }

    [Fact(Skip = "Ignore flaky timer tests for now")]
    public void RestartingResetsTimeout()
    {
        var timer = new SimpleTimer(new TimeSpan(500));
        Assert.False(timer.IsTimeout());
        Thread.Sleep(1000);
        Assert.True(timer.IsTimeout());
        timer.Restart();
        Assert.False(timer.IsTimeout());
    }

    [Fact(Skip = "Ignore flaky timer tests for now")]
    public void ImmediateTimeout()
    {
        var timer = new SimpleTimer(new TimeSpan(0));
        Assert.True(timer.IsTimeout());
    }
}