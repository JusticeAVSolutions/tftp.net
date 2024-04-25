using System.Net;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Transfer.States;

class SendReadRequest : StateWithNetworkTimeout
{
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        SendRequest(); //Send a read request to the server
    }

    private void SendRequest()
    {
        var request = new ReadRequest(Context.Filename, Context.TransferMode, Context.ProposedOptions.ToOptionList());
        SendAndRepeat(request);
    }

    public override void OnCommand(ITftpCommand command, EndPoint endpoint)
    {
        if (command is Data or OptionAcknowledgement)
        {
            //The server acknowledged our read request.
            //Fix out remote endpoint
            Context.GetConnection().RemoteEndpoint = endpoint;
        }

        switch (command)
        {
            case Data:
            {
                if (Context.NegotiatedOptions == null)
                    Context.FinishOptionNegotiation(TransferOptionSet.NewEmptySet());

                //Switch to the receiving state...
                ITransferState nextState = new Receiving();
                Context.SetState(nextState);

                //...and let it handle the data packet
                nextState.OnCommand(command, endpoint);
                break;
            }
            case OptionAcknowledgement optionAcknowledgement:
                //Check which options were acknowledged
                Context.FinishOptionNegotiation(new TransferOptionSet(optionAcknowledgement.Options));

                //the server acknowledged our options. Confirm the final options
                SendAndRepeat(new Acknowledgement(0));
                break;
            case Error error:
                Context.SetState(new ReceivedError(error));
                break;
            default:
                base.OnCommand(command, endpoint);
                break;
        }
    }

    public override void OnCancel(TftpErrorPacket reason)
    {
        Context.SetState(new CancelledByUser(reason));
    }
}