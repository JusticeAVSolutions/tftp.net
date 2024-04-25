﻿using NUnit.Framework;
using Tftp.Net.Transfer.States;

namespace Tftp.Net.UnitTests;

[TestFixture]
internal class ClosedState_Test
{
    private TransferStub transfer;

    [SetUp]
    public void Setup()
    {
        transfer = new TransferStub();
        transfer.SetState(new Closed());
    }

    [Test]
    public void CanNotCancel()
    {
        transfer.Cancel(TftpErrorPacket.IllegalOperation);
        Assert.IsInstanceOf<Closed>(transfer.State);
    }

    [Test]
    public void IgnoresCommands()
    {
        transfer.OnCommand(new Error(10, "Test"));
        Assert.IsInstanceOf<Closed>(transfer.State);
    }
}