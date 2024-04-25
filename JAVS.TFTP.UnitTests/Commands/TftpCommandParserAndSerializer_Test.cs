using System;
using System.IO;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.UnitTests.Commands;

public class TftpCommandParserAndSerializer_Test
{
    private static byte[] Serialize(ITftpCommand command)
    {
        using var stream = new MemoryStream();
        var serializer = new CommandSerializer();
        serializer.Serialize(command, stream);
        var commandAsBytes = stream.GetBuffer();
        Array.Resize(ref commandAsBytes, (int)stream.Length);
        return commandAsBytes;
    }

    [Fact]
    public void ParsesAck()
    {
        var original = new Acknowledgement(10);

        var parsed = (Acknowledgement)CommandParser.Parse(Serialize(original));
        Assert.Equal(original.BlockNumber, parsed.BlockNumber);
    }

    [Fact]
    public void ParsesError()
    {
        var original = new Error(15, "Hallo Welt");

        var parsed = (Error)CommandParser.Parse(Serialize(original));
        Assert.Equal(original.ErrorCode, parsed.ErrorCode);
        Assert.Equal(original.Message, parsed.Message);
    }

    [Fact]
    public void ParsesReadRequest()
    {
        var original = new ReadRequest("Hallo Welt.txt", TftpTransferMode.netascii, null);

        var parsed = (ReadRequest)CommandParser.Parse(Serialize(original));
        Assert.Equal(original.Filename, parsed.Filename);
        Assert.Equal(original.Mode, parsed.Mode);
    }

    [Fact]
    public void ParsesWriteRequest()
    {
        var original = new WriteRequest("Hallo Welt.txt", TftpTransferMode.netascii, null);

        var parsed = (WriteRequest)CommandParser.Parse(Serialize(original));
        Assert.Equal(original.Filename, parsed.Filename);
        Assert.Equal(original.Mode, parsed.Mode);
    }

    [Fact]
    public void ParsesData()
    {
        byte[] data = [12, 15, 19, 0, 4];
        var original = new Data(123, data);

        var parsed = (Data)CommandParser.Parse(Serialize(original));
        Assert.Equal(original.BlockNumber, parsed.BlockNumber);
        Assert.Equal(original.Bytes.Length, parsed.Bytes.Length);

        for (int i = 0; i < original.Bytes.Length; i++)
            Assert.Equal(original.Bytes[i], parsed.Bytes[i]);
    }
}