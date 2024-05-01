using System;
using System.Diagnostics;

namespace JAVS.TFTP.Transfer;

/// <summary>
/// Simple implementation of a timer.
/// </summary>
internal class SimpleTimer
{
    private readonly long _timeoutTicks;
    private long _nextTimeoutTicks;

    public SimpleTimer(TimeSpan timeout)
    {
        _timeoutTicks = timeout.Ticks;
        Restart();
    }

    public void Restart()
    {
        var ticks = Stopwatch.GetTimestamp();
        _nextTimeoutTicks = ticks + _timeoutTicks;
    }

    public bool IsTimeout()
    {
        var ticks = Stopwatch.GetTimestamp();
        return ticks >= _nextTimeoutTicks;
    }
}