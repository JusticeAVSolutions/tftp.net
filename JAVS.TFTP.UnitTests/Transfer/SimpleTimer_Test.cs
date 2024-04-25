using System;
using Tftp.Net.Transfer;
using System.Threading.Tasks;

namespace Tftp.Net.UnitTests.Transfer;

public class SimpleTimer_Test
{
    [Fact]
    public async Task TimesOutWhenTimeoutIsReached()
    {
        SimpleTimer timer = new SimpleTimer(new TimeSpan(100));
        Assert.False(timer.IsTimeout());
        await Task.Delay(200);
        Assert.True(timer.IsTimeout());
    }

    [Fact]
    public async Task RestartingResetsTimeout()
    {
        SimpleTimer timer = new SimpleTimer(new TimeSpan(100));
        Assert.False(timer.IsTimeout());
        await Task.Delay(200);
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