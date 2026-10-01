using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// A pick up that gives the player ammo.
public class seAmmo : sePickUp
{
    [ksEditable]
    public AmmoTypes AmmoType;

    [ksEditable]
    public int Amount = 20;

    protected override bool PickUp(seCharacter character)
    {
        character.AddAmmo(AmmoType, Amount);
        return true;
    }
}