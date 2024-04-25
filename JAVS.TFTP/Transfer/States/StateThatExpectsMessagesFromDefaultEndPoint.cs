using System;
using System.Net;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Transfer.States;

class StateThatExpectsMessagesFromDefaultEndPoint : StateWithNetworkTimeout, ITftpCommandVisitor
{
    public override void OnCommand(ITftpCommand command, EndPoint endpoint)
    {
        if (!endpoint.Equals(Context.GetConnection().RemoteEndpoint))
            throw new Exception("Received message from illegal endpoint. Actual: " + endpoint + ". Expected: " +
                                Context.GetConnection().RemoteEndpoint);

        command.Visit(this);
    }

    public virtual void OnReadRequest(ReadRequest command)
    {
        throw new NotImplementedException();
    }

    public virtual void OnWriteRequest(WriteRequest command)
    {
        throw new NotImplementedException();
    }

    public virtual void OnData(Data command)
    {
        throw new NotImplementedException();
    }

    public virtual void OnAcknowledgement(Acknowledgement command)
    {
        throw new NotImplementedException();
    }

    public virtual void OnError(Error command)
    {
        throw new NotImplementedException();
    }

    public virtual void OnOptionAcknowledgement(OptionAcknowledgement command)
    {
        throw new NotImplementedException();
    }
}