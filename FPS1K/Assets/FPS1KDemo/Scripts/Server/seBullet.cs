using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// A bullet that has a velocity and optionally gravity. It travels until it hits something. It applies an impulse to
// physics objects that it hits and damages players.
public class seBullet : ksServerEntityScript
{
    // The bullet destroys itself in this many seconds if it doesn't hit anything.
    [ksEditable]
    public float LifeTime = 10f;
    [ksEditable]
    public float Radius = .05f;

    public ksIServerEntity Owner;
    public ksVector3 Velocity;
    public float GravityMultiplier;
    public int Damage;
    public float Impulse;

    // The bullet cannot hit the player who fired it for this many seconds after being fired.
    private float m_preventOwnerHitTimer = .5f;
    private ksSweepParams m_sweepArgs = new ksSweepParams();


    // Called when the script is attached.
    public override void Initialize()
    {
        m_sweepArgs.Shape = new ksSphere(Radius);
        m_sweepArgs.Flags &= ~ksQueryFlags.EXCLUDE_OVERLAPS;
        m_sweepArgs.Filter = new ksGroupMaskFilter(Collision.SOLID);
        Room.OnUpdate[0] += Update;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }
    
    // Called during the update cycle
    protected virtual void Update()
    {
        LifeTime -= Time.Delta;
        if (LifeTime <= 0f)
        {
            Entity.Destroy();
            return;
        }
        Move();
    }

    protected void Move()
    {
        if (m_preventOwnerHitTimer > 0f)
        {
            m_preventOwnerHitTimer -= Time.Delta;
        }
        ksVector3 destination = Transform.Position + Velocity * Time.Delta;
        Velocity += Physics.Gravity * GravityMultiplier * Time.Delta;
        ksSweepResult hit;
        // Do a sweep from current position to the next position to see if we hit anything.
        m_sweepArgs.Origin = Transform.Position;
        m_sweepArgs.End = destination;
        m_sweepArgs.ExcludeEntity = m_preventOwnerHitTimer > 0f ? Owner : null;
        if (Physics.SweepNearest(m_sweepArgs, out hit))
        {
            if (hit.Distance > 0f)
            {
                Transform.Position += m_sweepArgs.Direction * m_sweepArgs.Distance;
            }
            else
            {
                // Point is (0, 0, 0) for initial overlaps. Use the entity's position as the hit point.
                hit.Point = Transform.Position;
            }
            if (Hit(hit))
            {
                Entity.Destroy();
            }
            return;
        }
        Transform.Position = destination;
    }

    // Called when the bullet hits something. Return true to destroy the bullet.
    protected virtual bool Hit(ksSweepResult hit)
    {
        ksIServerEntity entity = (ksIServerEntity)hit.Entity;
        seCharacter character = entity.Scripts.Get<seCharacter>();
        if (character != null)
        {
            character.ApplyDamage(Damage, hit.Point, Owner.Owner);
            return true;
        }

        ksVector3 direction = Velocity.Normalized();
        ksVector3 rebound = ksVector3.Reflect(direction, hit.Normal);
        Entity.Room.CallRPC(RPC.SHOOT, hit.Point, rebound);
        ksRigidBody rigidBody = entity.Scripts.Get<ksRigidBody>();
        if (rigidBody != null && !rigidBody.IsKinematic)
        {
            rigidBody.AddForceAtPosition(direction * Impulse, hit.Point, ksForceMode.IMPULSE);
        }
        return true;
    }
}