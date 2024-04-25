namespace Tftp.Net.UnitTests;

public class TftpCommand_Test
{
    [Fact]
    public void CreateAck()
    {
        Acknowledgement command = new Acknowledgement(100);
        Assert.Equal(100, command.BlockNumber);
    }

    [Fact]
    public void CreateError()
    {
        Error command = new Error(123, "Hallo Welt");
        Assert.Equal(123, command.ErrorCode);
        Assert.Equal("Hallo Welt", command.Message);
    }

    [Fact]
    public void CreateReadRequest()
    {
        ReadRequest command = new ReadRequest(@"C:\bla\blub.txt", TftpTransferMode.octet, null);
        Assert.Equal(@"C:\bla\blub.txt", command.Filename);
        Assert.Equal(TftpTransferMode.octet, command.Mode);
    }

    [Fact]
    public void CreateWriteRequest()
    {
        WriteRequest command = new WriteRequest(@"C:\bla\blub.txt", TftpTransferMode.octet, null);
        Assert.Equal(@"C:\bla\blub.txt", command.Filename);
        Assert.Equal(TftpTransferMode.octet, command.Mode);
    }

    [Fact]
    public void CreateData()
    {
        byte[] data = new byte[] { 1, 2, 3 };
        Data command = new Data(150, data);
        Assert.Equal(150, command.BlockNumber);
        Assert.Equal(data, command.Bytes);
    }
}