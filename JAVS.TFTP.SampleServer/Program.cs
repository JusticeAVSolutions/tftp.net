using System;
using System.IO;
using System.Net;
using Tftp.Net;

var serverDirectory = Environment.CurrentDirectory;

Console.WriteLine("Running TFTP server for directory: " + serverDirectory);
Console.WriteLine();
Console.WriteLine("Press any key to close the server.");

using (var server = new TftpServer())
{
    server.OnReadRequest += OnReadRequest;
    server.OnWriteRequest += OnWriteRequest;
    server.Start();
    Console.Read();
}

void OnWriteRequest(ITftpTransfer transfer, EndPoint client)
{
    var file = Path.Combine(serverDirectory, transfer.Filename);

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

void OnReadRequest(ITftpTransfer transfer, EndPoint client)
{
    var path = Path.Combine(serverDirectory, transfer.Filename);
    var file = new FileInfo(path);

    //Is the file within the server directory?
    if (!file.FullName.StartsWith(serverDirectory, StringComparison.InvariantCultureIgnoreCase))
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

void StartTransfer(ITftpTransfer transfer, Stream stream)
{
    transfer.OnProgress += OnProgress;
    transfer.OnError += OnError;
    transfer.OnFinished += OnFinished;
    transfer.Start(stream);
}

void CancelTransfer(ITftpTransfer transfer, TftpErrorPacket reason)
{
    OutputTransferStatus(transfer, "Cancelling transfer: " + reason.ErrorMessage);
    transfer.Cancel(reason);
}

void OnError(ITftpTransfer transfer, TftpTransferError error)
{
    OutputTransferStatus(transfer, "Error: " + error);
}

void OnFinished(ITftpTransfer transfer)
{
    OutputTransferStatus(transfer, "Finished");
}

void OnProgress(ITftpTransfer transfer, TftpTransferProgress progress)
{
    OutputTransferStatus(transfer, "Progress " + progress);
}

void OutputTransferStatus(ITftpTransfer transfer, string message)
{
    Console.WriteLine("[" + transfer.Filename + "] " + message);
}