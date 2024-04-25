using System.IO;

namespace Tftp.Net.UnitTests;

public class TftpStreamWriter_Test
{
    private readonly MemoryStream _ms;
    private readonly TftpStreamWriter _tested;

    public TftpStreamWriter_Test()
    {
        _ms = new MemoryStream();
        _tested = new TftpStreamWriter(_ms);
    }

    [Fact]
    public void WritesSingleBytes()
    {
        _tested.WriteByte(1);
        _tested.WriteByte(2);
        _tested.WriteByte(3);

        Assert.Equal(3, _ms.Length);
        Assert.Equal(1, _ms.GetBuffer()[0]);
        Assert.Equal(2, _ms.GetBuffer()[1]);
        Assert.Equal(3, _ms.GetBuffer()[2]);
    }

    [Fact]
    public void WritesShorts()
    {
        _tested.WriteUInt16(0x0102);
        _tested.WriteUInt16(0x0304);

        Assert.Equal(4, _ms.Length);
        Assert.Equal(1, _ms.GetBuffer()[0]);
        Assert.Equal(2, _ms.GetBuffer()[1]);
        Assert.Equal(3, _ms.GetBuffer()[2]);
        Assert.Equal(4, _ms.GetBuffer()[3]);
    }

    [Fact]
    public void WritesArrays()
    {
        _tested.WriteBytes([3, 4, 5]);

        Assert.Equal(3, _ms.Length);
        Assert.Equal(3, _ms.GetBuffer()[0]);
        Assert.Equal(4, _ms.GetBuffer()[1]);
        Assert.Equal(5, _ms.GetBuffer()[2]);
    }
}