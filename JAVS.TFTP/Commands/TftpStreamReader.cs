using System;
using System.IO;

namespace JAVS.TFTP.Commands;

internal class TftpStreamReader
{
    private readonly Stream _stream;

    public TftpStreamReader(Stream stream)
    {
        _stream = stream;
    }

    public ushort ReadUInt16()
    {
        var byte1 = _stream.ReadByte();
        var byte2 = _stream.ReadByte();
        return (ushort)((byte)byte1 << 8 | (byte)byte2);
    }

    public byte ReadByte()
    {
        var nextByte = _stream.ReadByte();

        if (nextByte == -1)
            throw new IOException();

        return (byte)nextByte;
    }

    public byte[] ReadBytes(int maxBytes)
    {
        var buffer = new byte[maxBytes];
        var bytesRead = _stream.Read(buffer, 0, buffer.Length);

        if (bytesRead == -1)
            throw new IOException();

        Array.Resize(ref buffer, bytesRead);
        return buffer;
    }
}