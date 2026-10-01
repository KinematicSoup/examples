using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// A pick up that recovers health.
public class seHealth : sePickUp
{
    [ksEditable]
    public int Health = 40;

    public override void Detached()
    {
        base.Detached();
    }

    protected override bool PickUp(seCharacter character)
    {
        character.Health += Health;
        return true;
    }
}