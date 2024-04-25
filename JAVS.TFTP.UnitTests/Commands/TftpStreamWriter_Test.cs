using System.IO;

namespace Tftp.Net.UnitTests;

public class TftpStreamWriter_Test
{
    private MemoryStream ms;
    private TftpStreamWriter tested;

    public TftpStreamWriter_Test()
    {
        ms = new MemoryStream();
        tested = new TftpStreamWriter(ms);
    }

    [Fact]
    public void WritesSingleBytes()
    {
        tested.WriteByte(1);
        tested.WriteByte(2);
        tested.WriteByte(3);

        Assert.Equal(3, ms.Length);
        Assert.Equal(1, ms.GetBuffer()[0]);
        Assert.Equal(2, ms.GetBuffer()[1]);
        Assert.Equal(3, ms.GetBuffer()[2]);
    }

    [Fact]
    public void WritesShorts()
    {
        tested.WriteUInt16(0x0102);
        tested.WriteUInt16(0x0304);

        Assert.Equal(4, ms.Length);
        Assert.Equal(1, ms.GetBuffer()[0]);
        Assert.Equal(2, ms.GetBuffer()[1]);
        Assert.Equal(3, ms.GetBuffer()[2]);
        Assert.Equal(4, ms.GetBuffer()[3]);
    }

    [Fact]
    public void WritesArrays()
    {
        tested.WriteBytes(new byte[3] { 3, 4, 5 });

        Assert.Equal(3, ms.Length);
        Assert.Equal(3, ms.GetBuffer()[0]);
        Assert.Equal(4, ms.GetBuffer()[1]);
        Assert.Equal(5, ms.GetBuffer()[2]);
    }
}