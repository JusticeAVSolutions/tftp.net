using System;
using System.IO;
using System.Net;

namespace Tftp.Net.SampleServer;

internal static class Program
{
    private static string ServerDirectory;

    private static void Main(string[] args)
    {
        ServerDirectory = Environment.CurrentDirectory;

        Console.WriteLine("Running TFTP server for directory: " + ServerDirectory);
        Console.WriteLine();
        Console.WriteLine("Press any key to close the server.");

        using (var server = new TftpServer())
        {
            server.OnReadRequest += server_OnReadRequest;
            server.OnWriteRequest += server_OnWriteRequest;
            server.Start();
            Console.Read();
        }
    }

    private static void server_OnWriteRequest(ITftpTransfer transfer, EndPoint client)
    {
        var file = Path.Combine(ServerDirectory, transfer.Filename);

        if (File.Exists(file))
        {
            CancelTransfer(transfer, TftpErrorPacket.FileAlreadyExists);
        }
        else
        {
            OutputTransferStatus(transfer, "Accepting write request from " + client);
            StartTransfer(transfer, new FileStream(file, FileMode.CreateNew));
        }
    }

    private static void server_OnReadRequest(ITftpTransfer transfer, EndPoint client)
    {
        var path = Path.Combine(ServerDirectory, transfer.Filename);
        var file = new FileInfo(path);

        //Is the file within the server directory?
        if (!file.FullName.StartsWith(ServerDirectory, StringComparison.InvariantCultureIgnoreCase))
        {
            CancelTransfer(transfer, TftpErrorPacket.AccessViolation);
        }
        else if (!file.Exists)
        {
            CancelTransfer(transfer, TftpErrorPacket.FileNotFound);
        }
        else
        {
            OutputTransferStatus(transfer, "Accepting request from " + client);
            StartTransfer(transfer, new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
        }
    }

    private static void StartTransfer(ITftpTransfer transfer, Stream stream)
    {
        transfer.OnProgress += transfer_OnProgress;
        transfer.OnError += transfer_OnError;
        transfer.OnFinished += transfer_OnFinished;
        transfer.Start(stream);
    }

    private static void CancelTransfer(ITftpTransfer transfer, TftpErrorPacket reason)
    {
        OutputTransferStatus(transfer, "Cancelling transfer: " + reason.ErrorMessage);
        transfer.Cancel(reason);
    }

    private static void transfer_OnError(ITftpTransfer transfer, TftpTransferError error)
    {
        OutputTransferStatus(transfer, "Error: " + error);
    }

    private static void transfer_OnFinished(ITftpTransfer transfer)
    {
        OutputTransferStatus(transfer, "Finished");
    }

    private static void transfer_OnProgress(ITftpTransfer transfer, TftpTransferProgress progress)
    {
        OutputTransferStatus(transfer, "Progress " + progress);
    }

    private static void OutputTransferStatus(ITftpTransfer transfer, string message)
    {
        Console.WriteLine("[" + transfer.Filename + "] " + message);
    }
}