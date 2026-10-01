using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor;
using KS.Reactor.Server;

// Base class for weapons that can be equipped by a player. Weapon assets should be created in Assets/Weapons, and
// corresponding prefabs for the client should have the same name and be placed in Assets/Resources/Weapons.
public abstract class sWeapon : ksScriptAsset, IWeapon
{
    public seCharacter Character
    {
        get { return m_character; }
        set { m_character = value; }
    }
    private seCharacter m_character;

    public ksIServerEntity Entity
    {
        get { return m_character == null ? null : m_character.Entity; }
    }

    public ksTime Time
    {
        get { return m_character == null ? null : m_character.Time; }
    }

    public virtual void Equip()
    {

    }

    public virtual void Unequip()
    {

    }

    // Called from the player controller Update.
    public virtual void OnUpdate(ksInput input)
    {

    }
}