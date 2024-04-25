using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JAVS.TFTP.Commands;

/// <summary>
/// Parses a ITftpCommand.
/// </summary>
internal static class CommandParser
{
    /// <summary>
    /// Parses an ITftpCommand from the given byte array. If the byte array cannot be parsed for some reason, a TftpParserException is thrown.
    /// </summary>
    public static ITftpCommand Parse(byte[] message)
    {
        try
        {
            return ParseInternal(message);
        }
        catch (TftpParserException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new TftpParserException(e);
        }
    }

    private static ITftpCommand ParseInternal(byte[] message)
    {
        var reader = new TftpStreamReader(new MemoryStream(message));

        var opcode = reader.ReadUInt16();
        return opcode switch
        {
            ReadRequest.OpCode => ParseReadRequest(reader),
            WriteRequest.OpCode => ParseWriteRequest(reader),
            Data.OpCode => ParseData(reader),
            Acknowledgement.OpCode => ParseAcknowledgement(reader),
            Error.OpCode => ParseError(reader),
            OptionAcknowledgement.OpCode => ParseOptionAcknowledgement(reader),
            _ => throw new TftpParserException("Invalid opcode")
        };
    }

    private static OptionAcknowledgement ParseOptionAcknowledgement(TftpStreamReader reader)
    {
        IEnumerable<TransferOption> options = ParseTransferOptions(reader);
        return new OptionAcknowledgement(options);
    }

    private static Error ParseError(TftpStreamReader reader)
    {
        var errorCode = reader.ReadUInt16();
        string message = ParseNullTerminatedString(reader);
        return new Error(errorCode, message);
    }

    private static Acknowledgement ParseAcknowledgement(TftpStreamReader reader)
    {
        var blockNumber = reader.ReadUInt16();
        return new Acknowledgement(blockNumber);
    }

    private static Data ParseData(TftpStreamReader reader)
    {
        var blockNumber = reader.ReadUInt16();
        var data = reader.ReadBytes(10000);
        return new Data(blockNumber, data);
    }

    private static WriteRequest ParseWriteRequest(TftpStreamReader reader)
    {
        string filename = ParseNullTerminatedString(reader);
        var mode = ParseModeType(ParseNullTerminatedString(reader));
        IEnumerable<TransferOption> options = ParseTransferOptions(reader);
        return new WriteRequest(filename, mode, options);
    }

    private static ReadRequest ParseReadRequest(TftpStreamReader reader)
    {
        string filename = ParseNullTerminatedString(reader);
        var mode = ParseModeType(ParseNullTerminatedString(reader));
        IEnumerable<TransferOption> options = ParseTransferOptions(reader);
        return new ReadRequest(filename, mode, options);
    }

    private static List<TransferOption> ParseTransferOptions(TftpStreamReader reader)
    {
        List<TransferOption> options = [];

        while (true)
        {
            String name;

            try
            {
                name = ParseNullTerminatedString(reader);
            }
            catch (IOException)
            {
                name = "";
            }

            if (name.Length == 0)
                break;

            var value = ParseNullTerminatedString(reader);
            options.Add(new TransferOption(name, value));
        }

        return options;
    }

    private static string ParseNullTerminatedString(TftpStreamReader reader)
    {
        byte b;
        var str = new StringBuilder();
        while ((b = reader.ReadByte()) > 0)
        {
            str.Append((char)b);
        }

        return str.ToString();
    }

    private static TftpTransferMode ParseModeType(string mode)
    {
        mode = mode.ToLowerInvariant();

        return mode switch
        {
            "netascii" => TftpTransferMode.netascii,
            "mail" => TftpTransferMode.mail,
            "octet" => TftpTransferMode.octet,
            _ => throw new TftpParserException("Unknown mode type: " + mode)
        };
    }
}

[Serializable]
class TftpParserException : Exception
{
    public TftpParserException(String message)
        : base(message)
    {
    }

    public TftpParserException(Exception e)
        : base("Error while parsing message.", e)
    {
    }
}