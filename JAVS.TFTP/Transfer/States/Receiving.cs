﻿using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Transfer.States;

internal class Receiving : StateThatExpectsMessagesFromDefaultEndPoint
{
    private ushort lastBlockNumber;
    private ushort nextBlockNumber = 1;
    private long bytesReceived;

    public override void OnData(Data command)
    {
        if (command.BlockNumber == nextBlockNumber)
        {
            //We received a new block of data
            Context.InputOutputStream.Write(command.Bytes, 0, command.Bytes.Length);
            SendAcknowledgement(command.BlockNumber);

            //Was that the last block of data?
            if (command.Bytes.Length < Context.BlockSize)
            {
                Context.RaiseOnFinished();
                Context.SetState(new Closed());
            }
            else
            {
                lastBlockNumber = command.BlockNumber;
                nextBlockNumber = Context.BlockCounterWrapping.CalculateNextBlockNumber(command.BlockNumber);
                bytesReceived += command.Bytes.Length;
                Context.RaiseOnProgress(bytesReceived);
            }
        }
        else if (command.BlockNumber == lastBlockNumber)
        {
            //We received the previous block again. Re-sent the acknowledgement
            SendAcknowledgement(command.BlockNumber);
        }
    }

    public override void OnCancel(TftpErrorPacket reason)
    {
        Context.SetState(new CancelledByUser(reason));
    }

    public override void OnError(Error command)
    {
        Context.SetState(new ReceivedError(command));
    }

    private void SendAcknowledgement(ushort blockNumber)
    {
        var ack = new Acknowledgement(blockNumber);
        Context.GetConnection().Send(ack);
        ResetTimeout();
    }

    public override void OnOptionAcknowledgement(OptionAcknowledgement command)
    {
    }
}