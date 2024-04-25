using System.IO;

namespace JAVS.TFTP.Commands;

class TftpStreamWriter
{
    private readonly Stream _stream;

    public TftpStreamWriter(Stream stream)
    {
        _stream = stream;
    }

    public void WriteUInt16(ushort value)
    {
        _stream.WriteByte((byte)(value >> 8));
        _stream.WriteByte((byte)(value & 0xFF));
    }

    public void WriteByte(byte b)
    {
        _stream.WriteByte(b);
    }

    public void WriteBytes(byte[] data)
    {
        _stream.Write(data, 0, data.Length);
    }
}