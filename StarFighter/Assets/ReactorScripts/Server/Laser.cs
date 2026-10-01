using System;
using System.Collections.Generic;
using KS.Reactor;
using KS.Reactor.Server;

/*
 * Controls laser bullets.
 */
public class Laser : ksServerEntityScript
{
    private const float PLAYER_HIT_RADIUS = .1f;

    // how much energy is required to fire this projectile
    [ksEditable]
    public int EnergyCost = 15;
    // how much health is subtracted when hit by a laser
    [ksEditable]
    public int Damage = 75;
    // how long the laser lasts until dying in seconds
    [ksEditable]
    private float m_lifeTime = 2.0f;
    
    private Player m_owner;
    public Player Owner
    {
        get { return m_owner; }
    }

    private Timer m_timeLeft;
    private ksVector3 m_lastPosition;

    private static ksSweepParams m_sweepParams;
    
	public override void Initialize()
	{
        if (m_sweepParams == null)
        {
            m_sweepParams = new ksSweepParams()
            {
                Shape = new ksSphere(PLAYER_HIT_RADIUS),
                Filter = new ksGroupMaskFilter(COLLISION.FIGHTER)
            };
        }
        Entity.OnCollision += OnCollision;
        Room.OnUpdate[0] += Update;
        m_timeLeft = new Timer(Time, m_lifeTime, true);
        m_lastPosition = Transform.Position;
    }

    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
        Entity.OnCollision -= OnCollision;
    }

    /*
     * Destroys the laser if exists for too long.
     */
	public void Update()
    {
        if (GameManager.GameOver)
        {
            return;
        }

        m_timeLeft.Update();

        if (m_timeLeft.IsDone)
        {
            Entity.Destroy();
        }

        if (m_lastPosition != Transform.Position)
        {
            m_sweepParams.Origin = m_lastPosition;
            m_sweepParams.End = Transform.Position;
            m_sweepParams.ExcludeEntity = Entity;
            ksSweepResult hit;
            if (Physics.SweepNearest(m_sweepParams, out hit))
            {
                Fighter fighter = ((ksIServerEntity)hit.Entity).Scripts.Get<Fighter>();
                if (fighter != null)
                {
                    fighter.Hit(Entity, Damage, m_owner);
                }
            }
            m_lastPosition = Transform.Position;
        }
    }

    public void InitializeLaser(Player owner, float speed)
    {
        m_owner = owner;
        Properties[ID.PROP.LASER.OWNER] = owner.Player.Id;
        ksRigidBody rigidBody = Scripts.Get<ksRigidBody>();
        rigidBody.Velocity = Transform.Forward() * speed;
    }

    private void OnCollision(ksContact contact)
    {
        PlinkSound();
    }

    public void PlinkSound()
    {
        Entity.CallRPC(m_owner.Player, ID.RPC.PLINK);
    }
}