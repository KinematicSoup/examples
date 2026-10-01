using System;
using System.Collections.Generic;
using KS.Reactor;
using KS.Reactor.Server;

/*
 * Controls missiles.
 */
public class Missile : ksServerEntityScript
{
    // how much energy is required to fire this projectile
    [ksEditable]
    public int EnergyCost = 400;
    // how much health is subtracted when hit by a missile
    [ksEditable]
    public int Damage = 600;
    // how long the missile lasts until dying in seconds
    [ksEditable]
    private float m_lifeTime = 10.0f;
    // how fast the missile moves (units/sec)
    [ksEditable]
    private float m_speed = 11.0f;
    // how fast the missile turns in degrees per second.
    [ksEditable]
    private float m_maxTurnRate = 90f;

    private Player m_owner;
    public Player Owner
    {
        get { return m_owner; }
    }

    private Timer m_timeLeft;
    private ksIServerEntity m_target;
    private ksRigidBody m_rigidBody;
    private static ksRaycastParams m_raycastParams;


	public override void Initialize()
    {
        if (m_raycastParams == null)
        {
            m_raycastParams = new ksRaycastParams()
            {
                Filter = new ksGroupMaskFilter(COLLISION.ENVIRONMENT)
            };
        }

        Room.OnUpdate[0] += Update;
        Entity.OnCollision += OnCollision;
        m_rigidBody = Scripts.Get<ksRigidBody>();

        m_timeLeft = new Timer(Time, m_lifeTime, true);
	}

    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
        Entity.OnCollision -= OnCollision;
    }

    /*
     * if we have existed for too long explode, otherwise turn towards the target.
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
            Explode();
        }
        
        if (m_target != null)
        {
            ksVector3 tp = m_target.Transform.Position;
            ksVector3 wp = tp;
            m_raycastParams.ExcludeEntity = Entity;
            m_raycastParams.Origin = Transform.Position;
            m_raycastParams.End = tp;
            bool canSeeTarget = true;
            float offsetRadius = 0.0f;
            ksRaycastResult hit;
            if (Physics.RaycastNearest(m_raycastParams, out hit))
            {
                if (hit.Entity != m_target)
                {
                    offsetRadius = hit.Entity.Transform.Scale.X * 2.0f;
                    wp = hit.Entity.Transform.Position;
                    canSeeTarget = false;
                }
            }

            // Choose the best waypoint direction
            if (!canSeeTarget)
            {
                ksVector3 offset = ksVector3.Zero;
                ksVector3 bestOffset = ksVector3.Zero;
                int bestCount = 100;

                // Up
                offset = Transform.Up() * offsetRadius;
                m_raycastParams.Origin = wp + offset;
                m_raycastParams.End = tp;
                int count = Physics.Raycast(m_raycastParams).Count;
                if (bestCount > count)
                {
                    bestOffset = wp + offset;
                    bestCount = count;
                }

                // Down
                offset = Transform.Down() * offsetRadius;
                m_raycastParams.Origin = wp + offset;
                m_raycastParams.End = tp;
                count = Physics.Raycast(m_raycastParams).Count;
                if (bestCount > count)
                {
                    bestOffset = wp + offset;
                    bestCount = count;
                }

                // Left
                offset = Transform.Left() * offsetRadius;
                m_raycastParams.Origin = wp + offset;
                m_raycastParams.End = tp;
                count = Physics.Raycast(m_raycastParams).Count;
                if (bestCount > count)
                {
                    bestOffset = wp + offset;
                    bestCount = count;
                }

                // Right
                offset = Transform.Right() * offsetRadius;
                m_raycastParams.Origin = wp + offset;
                m_raycastParams.End = tp;
                count = Physics.Raycast(m_raycastParams).Count;
                if (bestCount > count)
                {
                    bestOffset = wp + offset;
                    bestCount = count;
                }

                wp = bestOffset;
            }

            Transform.RotateTowards(wp, m_maxTurnRate * Time.Delta);
        }

        m_rigidBody.Velocity = Transform.Forward() * m_speed;
    }

    public void InitializeMissile(ksIServerEntity target, Player owner)
    {
        m_owner = owner;
        m_target = target;
        Properties[ID.PROP.MISSILE.OWNER] = owner.Player.Id;
    }

    private void OnCollision(ksContact contact)
    {
        Explode();
    }

    private void Explode()
    {
        Physics.ApplyExplosiveForce(Transform.Position, 5, 50, ksMath.Interpolation.EASE_OUT_SIN);

        Entity.Destroy();
    }
}