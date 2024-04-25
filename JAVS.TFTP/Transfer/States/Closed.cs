namespace JAVS.TFTP.Transfer.States;

internal class Closed : BaseState
{
    public override void OnStateEnter()
    {
        Context.Dispose();
    }
}