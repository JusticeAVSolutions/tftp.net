using System.IO;

namespace Tftp.Net.UnitTests;

public class TftpStreamReader_Test
{
    private byte[] Data = { 0x00, 0x01, 0x02, 0x03 };
    private TftpStreamReader tested;

    public TftpStreamReader_Test()
    {
        MemoryStream ms = new MemoryStream(Data);
        tested = new TftpStreamReader(ms);
    }

    [Fact]
    public void ReadsSingleBytes()
    {
        Assert.Equal(0x00, tested.ReadByte());
        Assert.Equal(0x01, tested.ReadByte());
        Assert.Equal(0x02, tested.ReadByte());
        Assert.Equal(0x03, tested.ReadByte());
    }

    [Fact]
    public void ReadsShorts()
    {
        Assert.Equal(0x0001, tested.ReadUInt16());
        Assert.Equal(0x0203, tested.ReadUInt16());
    }

    [Fact]
    public void ReadsIntoSmallerArrays()
    {
        byte[] bytes = tested.ReadBytes(2);
        Assert.Equal(2, bytes.Length);
        Assert.Equal(0x00, bytes[0]);
        Assert.Equal(0x01, bytes[1]);
    }

    [Fact]
    public void ReadsIntoArraysWithPerfectSize()
    {
        byte[] bytes = tested.ReadBytes(4);
        Assert.Equal(4, bytes.Length);
        Assert.Equal(0x00, bytes[0]);
        Assert.Equal(0x01, bytes[1]);
        Assert.Equal(0x02, bytes[2]);
        Assert.Equal(0x03, bytes[3]);
    }

    [Fact]
    public void ReadsIntoLargerArrays()
    {
        byte[] bytes = tested.ReadBytes(10);
        Assert.Equal(4, bytes.Length);
        Assert.Equal(0x00, bytes[0]);
        Assert.Equal(0x01, bytes[1]);
        Assert.Equal(0x02, bytes[2]);
        Assert.Equal(0x03, bytes[3]);
    }
}