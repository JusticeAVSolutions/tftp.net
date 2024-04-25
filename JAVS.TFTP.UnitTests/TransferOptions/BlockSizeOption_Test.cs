using Tftp.Net.Transfer;

namespace Tftp.Net.UnitTests.TransferOptions;

public class BlockSizeOption_Test
{
    private TransferOptionSet options;

    [Fact]
    public void AcceptsRegularOption()
    {
        Parse(new TransferOption("blksize", "16"));
        Assert.True(options.IncludesBlockSizeOption);
        Assert.Equal(16, options.BlockSize);
    }

    [Fact]
    public void IgnoresUnknownOption()
    {
        Parse(new TransferOption("blub", "16"));
        Assert.Equal(512, options.BlockSize);
        Assert.False(options.IncludesBlockSizeOption);
    }

    [Fact]
    public void IgnoresInvalidValue()
    {
        Parse(new TransferOption("blksize", "not-a-number"));
        Assert.Equal(512, options.BlockSize);
        Assert.False(options.IncludesBlockSizeOption);
    }

    [Fact]
    public void AcceptsMinBlocksize()
    {
        Parse(new TransferOption("blksize", "8"));
        Assert.True(options.IncludesBlockSizeOption);

        Parse(new TransferOption("blksize", "7"));
        Assert.False(options.IncludesBlockSizeOption);
    }

    [Fact]
    public void AcceptsMaxBlocksize()
    {
        Parse(new TransferOption("blksize", "65464"));
        Assert.True(options.IncludesBlockSizeOption);

        Parse(new TransferOption("blksize", "65465"));
        Assert.False(options.IncludesBlockSizeOption);
    }

    private void Parse(TransferOption option)
    {
        options = new TransferOptionSet(new TransferOption[] { option });
    }
}