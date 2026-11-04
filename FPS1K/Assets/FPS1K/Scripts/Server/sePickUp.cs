using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// A collectible pickup. It falls slowly to the ground when spawned and stops moving once it hits the ground. It cannot
// be pushed by anything. Pickups should not have rigidbodies. The player controller detects pickup overlaps.
public abstract class sePickUp : ksServerEntityScript
{
    private const float GRAVITY = 2f;
    private const float MAX_FALL_SPEED = 2f;

    private float m_ySpeed;
    private ksSweepParams m_sweepArgs;

    // Invoked when the item is picked up.
    public event Action OnPickUp;

    // Called when the script is attached.
    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;
        m_sweepArgs = new ksSweepParams();
        m_sweepArgs.Entity = Entity;
        m_sweepArgs.ExcludeEntity = Entity;
        m_sweepArgs.Filter = Entity.CollisionFilter;
        m_sweepArgs.Flags |= ksQueryFlags.EXCLUDE_TOUCHES;
        m_sweepArgs.UseEntityPosition = true;
        m_sweepArgs.UseEntityRotation = true;
        m_sweepArgs.Direction = ksVector3.Down;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }
    
    // Called during the update cycle
    private void Update()
    {
        if (Entity.IsDestroyed)
        {
            return;
        }
        if (Time.Delta == 0f)
        {
            return;
        }
        if (m_ySpeed < MAX_FALL_SPEED)
        {
            m_ySpeed = Math.Min(m_ySpeed + GRAVITY * Time.Delta, MAX_FALL_SPEED);
        }
        if (m_ySpeed == 0f)
        {
            return;
        }
        m_sweepArgs.Distance = m_ySpeed * Time.Delta;
        ksSweepResult hit;
        if (Physics.SweepNearest(m_sweepArgs, out hit))
        {
            // Stop falling once it hits something.
            Room.OnUpdate[0] -= Update;
            Transform.Position += ksVector3.Down * hit.Distance;
        }
        else
        {
            Transform.Position = m_sweepArgs.End;
        }
    }

    public void HandlePickUp(seCharacter character)
    {
        if (Entity == null || Entity.IsDestroyed || character.Entity == null || character.Entity.IsDestroyed)
        {
            return;
        }
        if (PickUp(character))
        {
            Entity.Destroy();
            if (OnPickUp != null)
            {
                OnPickUp();
            }
        }
    }

    // Called when a player gets the pickup. Derived classes should implement this. Return true to destroy the pickup.
    protected abstract bool PickUp(seCharacter character);
}