using Tftp.Net.Transfer;

namespace Tftp.Net.UnitTests.TransferOptions;

public class TransferSizeOption_Test
{
    private TransferOptionSet options;

    [Fact]
    public void ReadsTransferSize()
    {
        Parse(new TransferOption("tsize", "0"));
        Assert.True(options.IncludesTransferSizeOption);
        Assert.Equal(0, options.TransferSize);
    }

    [Fact]
    public void RejectsNegativeTransferSize()
    {
        Parse(new TransferOption("tsize", "-1"));
        Assert.False(options.IncludesTransferSizeOption);
    }

    [Fact]
    public void RejectsNonIntegerTransferSize()
    {
        Parse(new TransferOption("tsize", "abc"));
        Assert.False(options.IncludesTransferSizeOption);
    }

    private void Parse(TransferOption option)
    {
        options = new TransferOptionSet(new TransferOption[] { option });
    }
}