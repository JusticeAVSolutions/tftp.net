using System;
using System.Diagnostics;
using Tftp.Net.Trace;

namespace Tftp.Net.UnitTests.Trace;

public class TftpTrace_Test : IDisposable
{
    private class TraceListenerMock : TraceListener
    {
        public bool WriteWasCalled = false;

        public override void Write(string message)
        {
            WriteWasCalled = true;
        }

        public override void WriteLine(string message)
        {
            WriteWasCalled = true;
        }
    }

    private TraceListenerMock listener;

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