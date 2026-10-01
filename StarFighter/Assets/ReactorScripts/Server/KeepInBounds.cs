using System;
using System.Collections.Generic;
using KS.Reactor;
using KS.Reactor.Server;
using KS.Reactor.Server.PhysX;

/**
 * Keeps entites inside of the map by directing the velocity inwards.
 */
public class KeepInBounds : ksServerEntityScript
{
    private const float BOUNDS_RADIUS = 120.0f;
    private ksRigidBody m_rigidBody;

    public override void Initialize()
    {
        m_rigidBody = Scripts.Get<ksRigidBody>();
        if (m_rigidBody != null)
        {
            Room.OnUpdate[0] += Update;
        }
    }

    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }

    public void Update()
	{
        if (GameManager.GameOver || Transform.Position.Magnitude() < BOUNDS_RADIUS - 1)
        {
            return;
        }

        ksVector3 dir = -Transform.Position.Normalized();

        m_rigidBody.Velocity = ksVector3.Lerp(m_rigidBody.Velocity, dir * 0.5f, Time.Delta / 3);
        m_rigidBody.AngularVelocity = ksVector3.Lerp(m_rigidBody.AngularVelocity, ksVector3.Zero, Time.Delta / 2);
	}
}