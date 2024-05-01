using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer;

namespace JAVS.TFTP.UnitTests.TransferOptions;

public class TimeoutIntervalOption_Test
{
    private TransferOptionSet _options;

    [Fact]
    public void AcceptsValidTimeout()
    {
        Parse(new TransferOption("timeout", "10"));
        Assert.True(_options.IncludesTimeoutOption);
        Assert.Equal(10, _options.Timeout);
    }

    [Fact]
    public void AcceptsMinTimeout()
    {
        Parse(new TransferOption("timeout", "1"));
        Assert.True(_options.IncludesTimeoutOption);
        Assert.Equal(1, _options.Timeout);
    }

    [Fact]
    public void AcceptsMaxTimeout()
    {
        Parse(new TransferOption("timeout", "255"));
        Assert.True(_options.IncludesTimeoutOption);
        Assert.Equal(255, _options.Timeout);
    }

    [Fact]
    public void RejectsTimeoutTooLow()
    {
        Parse(new TransferOption("timeout", "0"));
        Assert.False(_options.IncludesTimeoutOption);
        Assert.Equal(10, _options.Timeout);
    }

    [Fact]
    public void RejectsTimeoutTooHigh()
    {
        Parse(new TransferOption("timeout", "256"));
        Assert.False(_options.IncludesTimeoutOption);
        Assert.Equal(10, _options.Timeout);
    }

    [Fact]
    public void RejectsNonIntegerTimeout()
    {
        Parse(new TransferOption("timeout", "blub"));
        Assert.False(_options.IncludesTimeoutOption);
        Assert.Equal(10, _options.Timeout);
    }

    private void Parse(TransferOption option)
    {
        _options = new TransferOptionSet(new[] { option });
    }
}