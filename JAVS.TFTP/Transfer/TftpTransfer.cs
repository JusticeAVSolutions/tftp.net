﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using JAVS.TFTP.Channel;
using JAVS.TFTP.Commands;
using JAVS.TFTP.Trace;
using JAVS.TFTP.Transfer.States;

namespace JAVS.TFTP.Transfer;

class TftpTransfer : ITftpTransfer
{
    protected ITransferState state;
    protected readonly ITransferChannel connection;
    protected Timer timer;

    public TransferOptionSet ProposedOptions { get; set; }
    public TransferOptionSet NegotiatedOptions { get; private set; }
    public bool WasStarted { get; private set; }
    public Stream InputOutputStream { get; protected set; }

    public TftpTransfer(ITransferChannel connection, String filename, ITransferState initialState)
    {
        ProposedOptions = TransferOptionSet.NewDefaultSet();
        Filename = filename;
        RetryCount = 5;
        SetState(initialState);
        this.connection = connection;
        this.connection.OnCommandReceived += connection_OnCommandReceived;
        this.connection.OnError += connection_OnError;
        this.connection.Open();
        timer = new Timer(timer_OnTimer, null, 500, 500);
    }

    private void timer_OnTimer(object context)
    {
        try
        {
            lock (this)
            {
                state.OnTimer();
            }
        }
        catch (Exception e)
        {
            TftpTrace.Trace("Ignoring unhandled exception: " + e, this);
        }
    }

    private void connection_OnCommandReceived(ITftpCommand command, EndPoint endpoint)
    {
        lock (this)
        {
            state.OnCommand(command, endpoint);
        }
    }

    private void connection_OnError(TftpTransferError error)
    {
        lock (this)
        {
            RaiseOnError(error);
        }
    }

    internal virtual void SetState(ITransferState newState)
    {
        state = DecorateForLogging(newState);
        state.Context = this;
        state.OnStateEnter();
    }

    protected virtual ITransferState DecorateForLogging(ITransferState state)
    {
        return TftpTrace.Enabled ? new LoggingStateDecorator(state, this) : state;
    }

    internal ITransferChannel GetConnection()
    {
        return connection;
    }

    internal void RaiseOnProgress(long bytesTransferred)
    {
        OnProgress?.Invoke(this, new TftpTransferProgress(bytesTransferred, ExpectedSize));
    }

    internal void RaiseOnError(TftpTransferError error)
    {
        OnError?.Invoke(this, error);
    }

    internal void RaiseOnFinished()
    {
        OnFinished?.Invoke(this);
    }

    internal void FinishOptionNegotiation(TransferOptionSet negotiated)
    {
        NegotiatedOptions = negotiated;
        if (!NegotiatedOptions.IncludesBlockSizeOption)
            NegotiatedOptions.BlockSize = TransferOptionSet.DEFAULT_BLOCKSIZE;

        if (!NegotiatedOptions.IncludesTimeoutOption)
            NegotiatedOptions.Timeout = TransferOptionSet.DEFAULT_TIMEOUT_SECS;
    }

    public override string ToString() => $"{GetHashCode()} ({Filename})";

    internal void FillOrDisableTransferSizeOption()
    {
        try
        {
            ProposedOptions.TransferSize = (int)InputOutputStream.Length;
        }
        catch (NotSupportedException)
        {
        }
        finally
        {
            if (ProposedOptions.TransferSize <= 0)
                ProposedOptions.IncludesTransferSizeOption = false;
        }
    }

    #region ITftpTransfer

    public event TftpProgressHandler OnProgress;
    public event TftpEventHandler OnFinished;
    public event TftpErrorHandler OnError;

    public string Filename { get; }
    public int RetryCount { get; set; }
    public virtual TftpTransferMode TransferMode { get; set; }
    public object UserContext { get; set; }

    public virtual TimeSpan RetryTimeout
    {
        get => TimeSpan.FromSeconds(NegotiatedOptions?.Timeout ?? ProposedOptions.Timeout);
        set
        {
            ThrowExceptionIfTransferAlreadyStarted();
            ProposedOptions.Timeout = value.Seconds;
        }
    }

    public virtual long ExpectedSize
    {
        get => NegotiatedOptions?.TransferSize ?? ProposedOptions.TransferSize;
        set
        {
            ThrowExceptionIfTransferAlreadyStarted();
            ProposedOptions.TransferSize = value;
        }
    }

    public virtual int BlockSize
    {
        get => NegotiatedOptions?.BlockSize ?? ProposedOptions.BlockSize;
        set
        {
            ThrowExceptionIfTransferAlreadyStarted();
            ProposedOptions.BlockSize = value;
        }
    }

    private BlockCounterWrapAround wrapping = BlockCounterWrapAround.ToZero;

    public virtual BlockCounterWrapAround BlockCounterWrapping
    {
        get => wrapping;
        set
        {
            ThrowExceptionIfTransferAlreadyStarted();
            wrapping = value;
        }
    }

    private void ThrowExceptionIfTransferAlreadyStarted()
    {
        if (WasStarted)
            throw new InvalidOperationException(
                "You cannot change tftp transfer options after the transfer has been started.");
    }

    public void Start(Stream data)
    {
        if (WasStarted)
            throw new InvalidOperationException("This transfer has already been started.");

        WasStarted = true;
        InputOutputStream = data ?? throw new ArgumentNullException(nameof(data));

        lock (this)
        {
            state.OnStart();
        }
    }

    public void Cancel(TftpErrorPacket reason)
    {
        if (reason == null)
            throw new ArgumentNullException(nameof(reason));

        lock (this)
        {
            state.OnCancel(reason);
        }
    }

    public virtual void Dispose()
    {
        lock (this)
        {
            timer.Dispose();
            Cancel(new TftpErrorPacket(0, "ITftpTransfer has been disposed."));

            if (InputOutputStream != null)
            {
                InputOutputStream.Close();
                InputOutputStream = null;
            }

            connection.Dispose();
        }
    }

    #endregion
}