using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor;
using KS.Reactor.Server;

// Base class for guns that use ammo and have a cooldown after they fire.
[ksSharedData]
public abstract class sAmmoGun : sWeapon
{
    [ksEditable]
    public AmmoTypes AmmoType;
    [ksEditable]
    public float Cooldown = .25f;

    public int Ammo
    {
        get { return Character.GetAmmo(AmmoType); }
        set { Character.SetAmmo(AmmoType, value); }
    }

    private float m_timer = 0f;
    protected bool m_shoot = false;
    protected FPSController m_controller;
    private srEventQueue m_eventQueue;

    public override void Equip()
    {
        Entity.OnRPC[RPC.SHOOT] += OnRequestShoot;
        m_timer = Cooldown;
        m_controller = Entity.PlayerController as FPSController;
        m_eventQueue = Entity.Room.Scripts.Get<srEventQueue>();
    }

    public override void Unequip()
    {
        Entity.OnRPC[RPC.SHOOT] -= OnRequestShoot;
    }

    public override void OnUpdate(ksInput input)
    {
        if (m_timer > 0f)
        {
            m_timer -= Time.Delta;
            return;
        }
        // Handle bot input. Bots have unlimited ammo.
        if (Entity.Owner.IsVirtual && input.IsDown(Buttons.SHOOT))
        {
            m_shoot = true;
        }
        if (!m_shoot)
        {
            m_timer = 0f;
            return;
        }
        m_shoot = false;
        m_timer += Cooldown;
        m_eventQueue.Enqueue(Shoot);
    }

    // Derived classes should implement this with shoot logic.
    protected virtual void Shoot()
    {

    }

    private void OnRequestShoot(ksIServerPlayer player, ksMultiType[] args)
    {
        if (player != Entity.Owner)
        {
            return;
        }
        if (!Character.UnlimitedAmmo)
        {
            if (Ammo <= 0)
            {
                // The client and server both track ammo indepentantly, but it should stay in sync. If this happens,
                // something went wrong.
                ksLog.Warning(this, "Tried to fire without any ammo.");
                return;
            }
            Ammo--;
        }
        if (m_timer > Time.UnscaledDelta * 4)
        {
            // The client and server cooldowns got out of sync and the client is trying to shoot too early.
            // Tell the client to add extra time to their next cooldown.
            Entity.Room.CallRPC(player, RPC.ADD_TIME, m_timer);
        }
        m_shoot = true;
        OnRequestShoot(args);
    }

    // Derived classes should implement this. It is called when the client wants to shoot.
    protected virtual void OnRequestShoot(ksMultiType[] args)
    {

    }
}