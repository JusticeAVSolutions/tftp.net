using System;

namespace Tftp.Net.UnitTests;

public class ErrorFromRemoteEndpoint_Test
{
    [Fact]
    public void CanBeCreatedWithValidValues()
    {
        TftpErrorPacket error = new TftpErrorPacket(123, "Test Message");
        Assert.Equal(123, error.ErrorCode);
        Assert.Equal("Test Message", error.ErrorMessage);
    }

    [Fact]
    public void RejectsNullMessage()
    {
        Assert.Throws<ArgumentException>(() => new TftpErrorPacket(123, null));
    }

    [Fact]
    public void RejectsEmptyMessage()
    {
        Assert.Throws<ArgumentException>(() => new TftpErrorPacket(123, ""));
    }
}