using NUnit.Framework;
using Tftp.Net.Transfer.States;

namespace Tftp.Net.UnitTests;

[TestFixture]
internal class CancelledByUserState_Test
{
    private TransferStub transfer;

    [SetUp]
    public void Setup()
    {
        transfer = new TransferStub();
        transfer.SetState(new CancelledByUser(TftpErrorPacket.IllegalOperation));
    }

    [Test]
    public void SendsErrorToClient()
    {
        Assert.IsTrue(transfer.CommandWasSent(typeof(Error)));
    }

    [Test]
    public void TransitionsToClosedState()
    {
        Assert.IsInstanceOf<Closed>(transfer.State);
    }
}