using System;
using System.IO;
using System.Threading;
using JAVS.TFTP;

AutoResetEvent transferFinishedEvent = new(false);

//Setup a TftpClient instance
var client = new TftpClient("localhost");

//Prepare a simple transfer (GET test.dat)
var transfer = client.Download("EUPL-EN.pdf");

//Capture the events that may happen during the transfer
transfer.OnProgress += OnProgress;
transfer.OnFinished += OnFinished;
transfer.OnError += OnError;

//Start the transfer and write the data that we're downloading into a memory stream
Stream stream = new MemoryStream();
transfer.Start(stream);

//Wait for the transfer to finish
transferFinishedEvent.WaitOne();
Console.ReadKey();

void OnProgress(ITftpTransfer _, TftpTransferProgress progress)
{
    Console.WriteLine("Transfer running. Progress: " + progress);
}

void OnError(ITftpTransfer _, TftpTransferError error)
{
    Console.WriteLine("Transfer failed: " + error);
    transferFinishedEvent.Set();
}

void OnFinished(ITftpTransfer _)
{
    Console.WriteLine("Transfer succeeded.");
    transferFinishedEvent.Set();
}