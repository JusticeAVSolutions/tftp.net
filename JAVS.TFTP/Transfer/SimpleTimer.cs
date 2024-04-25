using System;

namespace JAVS.TFTP.Transfer;

/// <summary>
/// Simple implementation of a timer.
/// </summary>
internal class SimpleTimer
{
    private readonly TimeSpan _timeout;
    private DateTime _nextTimeout;

    public SimpleTimer(TimeSpan timeout)
    {
        _timeout = timeout;
        Restart();
    }

    public void Restart()
    {
        _nextTimeout = DateTime.Now.Add(_timeout);
    }

    public bool IsTimeout()
    {
        return DateTime.Now >= _nextTimeout;
    }
}