using System.IO;
using System.Text;

namespace JAVS.TFTP.Commands;

/// <summary>
/// Serializes an ITftpCommand into a stream of bytes.
/// </summary>
internal class CommandSerializer
{
    /// <summary>
    /// Call this method to serialize the given <code>command</code> using the given <code>writer</code>.
    /// </summary>
    public void Serialize(ITftpCommand command, Stream stream)
    {
        var visitor = new CommandComposerVisitor(stream);
        command.Visit(visitor);
    }

    private class CommandComposerVisitor : ITftpCommandVisitor
    {
        private readonly TftpStreamWriter _writer;

        public CommandComposerVisitor(Stream stream)
        {
            _writer = new TftpStreamWriter(stream);
        }

        private void OnReadOrWriteRequest(ReadOrWriteRequest command)
        {
            _writer.WriteBytes(Encoding.ASCII.GetBytes(command.Filename));
            _writer.WriteByte(0);
            _writer.WriteBytes(Encoding.ASCII.GetBytes(command.Mode.ToString()));
            _writer.WriteByte(0);

            if (command.Options != null)
            {
                foreach (var option in command.Options)
                {
                    _writer.WriteBytes(Encoding.ASCII.GetBytes(option.Name));
                    _writer.WriteByte(0);
                    _writer.WriteBytes(Encoding.ASCII.GetBytes(option.Value));
                    _writer.WriteByte(0);
                }
            }
        }

        public void OnReadRequest(ReadRequest command)
        {
            _writer.WriteUInt16(ReadRequest.OpCode);
            OnReadOrWriteRequest(command);
        }

        public void OnWriteRequest(WriteRequest command)
        {
            _writer.WriteUInt16(WriteRequest.OpCode);
            OnReadOrWriteRequest(command);
        }

        public void OnData(Data command)
        {
            _writer.WriteUInt16(Data.OpCode);
            _writer.WriteUInt16(command.BlockNumber);
            _writer.WriteBytes(command.Bytes);
        }

        public void OnAcknowledgement(Acknowledgement command)
        {
            _writer.WriteUInt16(Acknowledgement.OpCode);
            _writer.WriteUInt16(command.BlockNumber);
        }

        public void OnError(Error command)
        {
            _writer.WriteUInt16(Error.OpCode);
            _writer.WriteUInt16(command.ErrorCode);
            _writer.WriteBytes(Encoding.ASCII.GetBytes(command.Message));
            _writer.WriteByte(0);
        }

        public void OnOptionAcknowledgement(OptionAcknowledgement command)
        {
            _writer.WriteUInt16(OptionAcknowledgement.OpCode);

            foreach (var option in command.Options)
            {
                _writer.WriteBytes(Encoding.ASCII.GetBytes(option.Name));
                _writer.WriteByte(0);
                _writer.WriteBytes(Encoding.ASCII.GetBytes(option.Value));
                _writer.WriteByte(0);
            }
        }
    }
}