using Tftp.Net.Transfer;

namespace Tftp.Net.UnitTests.TransferOptions;

public class TimeoutIntervalOption_Test
{
    private TransferOptionSet options;

    [Fact]
    public void AcceptsValidTimeout()
    {
        Parse(new TransferOption("timeout", "10"));
        Assert.True(options.IncludesTimeoutOption);
        Assert.Equal(10, options.Timeout);
    }

    [Fact]
    public void AcceptsMinTimeout()
    {
        Parse(new TransferOption("timeout", "1"));
        Assert.True(options.IncludesTimeoutOption);
        Assert.Equal(1, options.Timeout);
    }

    [Fact]
    public void AcceptsMaxTimeout()
    {
        Parse(new TransferOption("timeout", "255"));
        Assert.True(options.IncludesTimeoutOption);
        Assert.Equal(255, options.Timeout);
    }

    [Fact]
    public void RejectsTimeoutTooLow()
    {
        Parse(new TransferOption("timeout", "0"));
        Assert.False(options.IncludesTimeoutOption);
        Assert.Equal(5, options.Timeout);
    }

    [Fact]
    public void RejectsTimeoutTooHigh()
    {
        Parse(new TransferOption("timeout", "256"));
        Assert.False(options.IncludesTimeoutOption);
        Assert.Equal(5, options.Timeout);
    }

    [Fact]
    public void RejectsNonIntegerTimeout()
    {
        Parse(new TransferOption("timeout", "blub"));
        Assert.False(options.IncludesTimeoutOption);
        Assert.Equal(5, options.Timeout);
    }

    private void Parse(TransferOption option)
    {
        options = new TransferOptionSet(new TransferOption[] { option });
    }
}