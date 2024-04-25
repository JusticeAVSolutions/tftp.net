using JAVS.TFTP.Commands;
using JAVS.TFTP.Transfer;

namespace JAVS.TFTP.UnitTests.TransferOptions;

public class TransferSizeOption_Test
{
    private TransferOptionSet _options;

    [Fact]
    public void ReadsTransferSize()
    {
        Parse(new TransferOption("tsize", "0"));
        Assert.True(_options.IncludesTransferSizeOption);
        Assert.Equal(0, _options.TransferSize);
    }

    [Fact]
    public void RejectsNegativeTransferSize()
    {
        Parse(new TransferOption("tsize", "-1"));
        Assert.False(_options.IncludesTransferSizeOption);
    }

    [Fact]
    public void RejectsNonIntegerTransferSize()
    {
        Parse(new TransferOption("tsize", "abc"));
        Assert.False(_options.IncludesTransferSizeOption);
    }

    private void Parse(TransferOption option)
    {
        _options = new TransferOptionSet(new[] { option });
    }
}