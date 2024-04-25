using System.IO;

namespace Tftp.Net.UnitTests;

public class TftpStreamReader_Test
{
    private readonly TftpStreamReader _tested = new(new MemoryStream([0x00, 0x01, 0x02, 0x03]));

    [Fact]
    public void ReadsSingleBytes()
    {
        Assert.Equal(0x00, _tested.ReadByte());
        Assert.Equal(0x01, _tested.ReadByte());
        Assert.Equal(0x02, _tested.ReadByte());
        Assert.Equal(0x03, _tested.ReadByte());
    }

    [Fact]
    public void ReadsShorts()
    {
        Assert.Equal(0x0001, _tested.ReadUInt16());
        Assert.Equal(0x0203, _tested.ReadUInt16());
    }

    [Fact]
    public void ReadsIntoSmallerArrays()
    {
        var bytes = _tested.ReadBytes(2);
        Assert.Equal(2, bytes.Length);
        Assert.Equal(0x00, bytes[0]);
        Assert.Equal(0x01, bytes[1]);
    }

    [Fact]
    public void ReadsIntoArraysWithPerfectSize()
    {
        var bytes = _tested.ReadBytes(4);
        Assert.Equal(4, bytes.Length);
        Assert.Equal(0x00, bytes[0]);
        Assert.Equal(0x01, bytes[1]);
        Assert.Equal(0x02, bytes[2]);
        Assert.Equal(0x03, bytes[3]);
    }

    [Fact]
    public void ReadsIntoLargerArrays()
    {
        var bytes = _tested.ReadBytes(10);
        Assert.Equal(4, bytes.Length);
        Assert.Equal(0x00, bytes[0]);
        Assert.Equal(0x01, bytes[1]);
        Assert.Equal(0x02, bytes[2]);
        Assert.Equal(0x03, bytes[3]);
    }
}