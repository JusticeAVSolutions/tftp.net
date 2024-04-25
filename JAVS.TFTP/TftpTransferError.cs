using System;

namespace JAVS.TFTP;

/// <summary>
/// Base class for all errors that may be passed to <code>ITftpTransfer.OnError</code>.
/// </summary>
public abstract class TftpTransferError
{
    public abstract override string ToString();
}

/// <summary>
/// Errors that are sent from the remote party using the TFTP Error Packet are represented
/// by this class.
/// </summary>
public class TftpErrorPacket : TftpTransferError
{
    /// <summary>
    /// Error code that was sent from the other party.
    /// </summary>
    public ushort ErrorCode { get; }

    /// <summary>
    /// Error description that was sent by the other party.
    /// </summary>
    public string ErrorMessage { get; }

    public TftpErrorPacket(ushort errorCode, string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            throw new ArgumentException("You must provide an errorMessage.");

        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public override string ToString() => $"{ErrorCode} - {ErrorMessage}";

    #region Predefined error packets from RFC 1350

    public static readonly TftpErrorPacket FileNotFound = new(1, "File not found");
    public static readonly TftpErrorPacket AccessViolation = new(2, "Access violation");
    public static readonly TftpErrorPacket DiskFull = new(3, "Disk full or allocation exceeded");
    public static readonly TftpErrorPacket IllegalOperation = new(4, "Illegal TFTP operation");
    public static readonly TftpErrorPacket UnknownTransferId = new(5, "Unknown transfer ID");
    public static readonly TftpErrorPacket FileAlreadyExists = new(6, "File already exists");
    public static readonly TftpErrorPacket NoSuchUser = new(7, "No such user");

    #endregion
}

/// <summary>
/// Network errors (i.e. socket exceptions) are represented by this class.
/// </summary>
public class NetworkError : TftpTransferError
{
    public Exception Exception { get; }

    public NetworkError(Exception exception)
    {
        Exception = exception;
    }

    public override string ToString()
    {
        return Exception.ToString();
    }
}

/// <summary>
/// $(ITftpTransfer.RetryTimeout) has been exceeded more than $(ITftpTransfer.RetryCount) times in a row.
/// </summary>
public class TimeoutError : TftpTransferError
{
    private readonly TimeSpan _retryTimeout;
    private readonly int _retryCount;

    public TimeoutError(TimeSpan retryTimeout, int retryCount)
    {
        _retryTimeout = retryTimeout;
        _retryCount = retryCount;
    }

    public override string ToString() =>
        $"Timeout error. RetryTimeout ({_retryTimeout}) violated more than {_retryCount} times in a row";
}