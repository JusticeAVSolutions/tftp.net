﻿namespace JAVS.TFTP.Trace;

/// <summary>
/// Class that controls all tracing in the TFTP module.
/// </summary>
public static class TftpTrace
{
    /// <summary>
    /// Set this property to <code>false</code> to disable tracing.
    /// </summary>
    public static bool Enabled { get; set; }

    static TftpTrace()
    {
        Enabled = false;
    }

    internal static void Trace(string message, ITftpTransfer transfer)
    {
        if (!Enabled)
            return;

        System.Diagnostics.Trace.WriteLine(message, transfer.ToString());
    }
}