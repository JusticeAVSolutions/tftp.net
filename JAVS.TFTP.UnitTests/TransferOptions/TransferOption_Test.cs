using System;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.UnitTests.TransferOptions;

public class TransferOption_Test
{
    [Fact]
    public void CanBeCreatedWithValidNameAndValue()
    {
        var option = new TransferOption("Test", "Hallo Welt");
        Assert.Equal("Test", option.Name);
        Assert.Equal("Hallo Welt", option.Value);
        Assert.False(option.IsAcknowledged);
    }

    [Fact]
    public void RejectsInvalidName1()
    {
        Assert.Throws<ArgumentException>(() => new TransferOption("", "Hallo Welt"));
    }

    [Fact]
    public void RejectsInvalidName2()
    {
        Assert.Throws<ArgumentException>(() => new TransferOption(null, "Hallo Welt"));
    }

    [Fact]
    public void RejectsInvalidValue()
    {
        Assert.Throws<ArgumentNullException>(() => new TransferOption("Test", null));
    }

    [Fact]
    public void AcceptsEmptyValue()
    {
        //Must not throw any exceptions
        var option = new TransferOption("Test", "");
    }
}