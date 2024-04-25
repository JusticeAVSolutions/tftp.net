using System.Collections.Generic;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Transfer.States;

internal class StartIncomingWrite : BaseState
{
    private readonly IEnumerable<TransferOption> _optionsRequestedByClient;

    public StartIncomingWrite(IEnumerable<TransferOption> optionsRequestedByClient)
    {
        _optionsRequestedByClient = optionsRequestedByClient;
    }

    public override void OnStateEnter()
    {
        Context.ProposedOptions = new TransferOptionSet(_optionsRequestedByClient);
    }

    public override void OnStart()
    {
        //Do we have any acknowledged options?
        Context.FinishOptionNegotiation(Context.ProposedOptions);
        var options = Context.NegotiatedOptions.ToOptionList();
        if (options.Count > 0)
        {
            Context.SetState(new SendOptionAcknowledgementForWriteRequest());
        }
        else
        {
            //Start receiving
            Context.SetState(new AcknowledgeWriteRequest());
        }
    }

    public override void OnCancel(TftpErrorPacket reason)
    {
        Context.SetState(new CancelledByUser(reason));
    }
}