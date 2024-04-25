using System;
using System.IO;
using System.Threading;

namespace Tftp.Net.SampleClient;

internal static class Program
{
    private static AutoResetEvent TransferFinishedEvent = new AutoResetEvent(false);

    private static void Main(string[] args)
    {
        //Setup a TftpClient instance
        var client = new TftpClient("localhost");

        //Prepare a simple transfer (GET test.dat)
        var transfer = client.Download("EUPL-EN.pdf");

        //Capture the events that may happen during the transfer
        transfer.OnProgress += transfer_OnProgress;
        transfer.OnFinished += transfer_OnFinished;
        transfer.OnError += transfer_OnError;

        //Start the transfer and write the data that we're downloading into a memory stream
        Stream stream = new MemoryStream();
        transfer.Start(stream);

        //Wait for the transfer to finish
        TransferFinishedEvent.WaitOne();
        Console.ReadKey();
    }

    private static void transfer_OnProgress(ITftpTransfer transfer, TftpTransferProgress progress)
    {
        Console.WriteLine("Transfer running. Progress: " + progress);
    }

    private static void transfer_OnError(ITftpTransfer transfer, TftpTransferError error)
    {
        Console.WriteLine("Transfer failed: " + error);
        TransferFinishedEvent.Set();
    }

    private static void transfer_OnFinished(ITftpTransfer transfer)
    {
        Console.WriteLine("Transfer succeeded.");
        TransferFinishedEvent.Set();
    }
}