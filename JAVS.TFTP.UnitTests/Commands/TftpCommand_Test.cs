using JAVS.TFTP.Commands;

namespace JAVS.TFTP.UnitTests.Commands;

public class TftpCommand_Test
{
    [Fact]
    public void CreateAck()
    {
        var command = new Acknowledgement(100);
        Assert.Equal(100, command.BlockNumber);
    }

    [Fact]
    public void CreateError()
    {
        var command = new Error(123, "Hallo Welt");
        Assert.Equal(123, command.ErrorCode);
        Assert.Equal("Hallo Welt", command.Message);
    }

    [Fact]
    public void CreateReadRequest()
    {
        var command = new ReadRequest(@"C:\bla\blub.txt", TftpTransferMode.octet, null);
        Assert.Equal(@"C:\bla\blub.txt", command.Filename);
        Assert.Equal(TftpTransferMode.octet, command.Mode);
    }

    [Fact]
    public void CreateWriteRequest()
    {
        var command = new WriteRequest(@"C:\bla\blub.txt", TftpTransferMode.octet, null);
        Assert.Equal(@"C:\bla\blub.txt", command.Filename);
        Assert.Equal(TftpTransferMode.octet, command.Mode);
    }

    [Fact]
    public void CreateData()
    {
        byte[] data = [1, 2, 3];
        var command = new Data(150, data);
        Assert.Equal(150, command.BlockNumber);
        Assert.Equal(data, command.Bytes);
    }
}