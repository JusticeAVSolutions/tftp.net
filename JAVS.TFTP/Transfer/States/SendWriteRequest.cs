namespace Tftp.Net.Transfer.States;

class SendWriteRequest : StateWithNetworkTimeout
{
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        SendRequest();
    }

    private void SendRequest()
    {
        var request = new WriteRequest(Context.Filename, Context.TransferMode, Context.ProposedOptions.ToOptionList());
        SendAndRepeat(request);
    }

    public override void OnCommand(ITftpCommand command, System.Net.EndPoint endpoint)
    {
        switch (command)
        {
            case OptionAcknowledgement optionAcknowledgement:
            {
                var acknowledged = new TransferOptionSet(optionAcknowledgement.Options);
                Context.FinishOptionNegotiation(acknowledged);
                BeginSendingTo(endpoint);
                break;
            }
            case Acknowledgement { BlockNumber: 0 }:
                Context.FinishOptionNegotiation(TransferOptionSet.NewEmptySet());
                BeginSendingTo(endpoint);
                break;
            case Error error:
                //The server denied our request
                Context.SetState(new ReceivedError(error));
                break;
            default:
                base.OnCommand(command, endpoint);
                break;
        }
    }

    private void BeginSendingTo(System.Net.EndPoint endpoint)
    {
        //Switch to the endpoint that we received from the server
        Context.GetConnection().RemoteEndpoint = endpoint;

        //Start sending packets
        Context.SetState(new Sending());
    }

    public override void OnCancel(TftpErrorPacket reason)
    {
        Context.SetState(new CancelledByUser(reason));
    }
}