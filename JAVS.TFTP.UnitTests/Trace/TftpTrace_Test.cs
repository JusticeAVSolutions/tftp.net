using System;
using System.Diagnostics;
using System.Threading;
using JAVS.TFTP.Trace;
using JAVS.TFTP.UnitTests.Transfer.States;

namespace JAVS.TFTP.UnitTests.Trace;

public class TftpTrace_Test : IDisposable
{
    private class TraceListenerMock : TraceListener
    {
        private volatile int _writeWasCalledInt;

        public bool WriteWasCalled => _writeWasCalledInt == 1;

        public override void Write(string message)
        {
            Interlocked.Exchange(ref _writeWasCalledInt, 1);
        }

        public override void WriteLine(string message)
        {
            Interlocked.Exchange(ref _writeWasCalledInt, 1);
        }
    }

    private readonly TraceListenerMock listener;

    public TftpTrace_Test()
    {
        listener = new TraceListenerMock();
        System.Diagnostics.Trace.Listeners.Add(listener);
    }

    public void Dispose()
    {
        System.Diagnostics.Trace.Listeners.Remove(listener);
        TftpTrace.Enabled = false;
    }

    [Fact]
    public void CallsTrace()
    {
        TftpTrace.Enabled = true;
        Assert.False(listener.WriteWasCalled);
        TftpTrace.Trace("Test", new TransferStub());
        Assert.True(listener.WriteWasCalled);
    }

    [Fact]
    public void DoesNotWriteWhenDisabled()
    {
        TftpTrace.Enabled = false;
        TftpTrace.Trace("Test", new TransferStub());
        Assert.False(listener.WriteWasCalled);
    }
}