using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Calculates the velocity and angular velocity of the entity from the difference in server positions between two
// server frames. This is used to get the velocity of the ground the player is standing on in the player controller.
public class ceVelocityTracker : ksEntityScript
{
    public ksVector3 Velocity
    {
        get { return m_velocity; }
    }

    public ksVector3 AngularVelocity
    {
        get { return m_angularVelocity; }
    }

    private ulong m_lastFrame;
    private ksVector3 m_lastPosition;
    private ksQuaternion m_lastRotation;
    private ksVector3 m_velocity;
    private ksVector3 m_angularVelocity;

    private static float m_serverTimeDelta = -1f;

    // Called after properties are initialized.
    public override void Initialize()
    {
        m_lastFrame = Time.Frame;
        m_lastPosition = Entity.ServerTransform.Position;
        m_lastRotation = Entity.ServerTransform.Rotation;
        if (m_serverTimeDelta < 0f)
        {
            m_serverTimeDelta = 1f / Room.RoomType.ServerFrameRate;
        }
    }

    // Called when the script is detached.
    public override void Detached()
    {

    }

    // Called every frame.
    private void Update()
    {
        if (Time.Frame == m_lastFrame)
        {
            return;
        }
        float dt = (Time.Frame - m_lastFrame) * m_serverTimeDelta;
        if (dt > 0f)
        {
            m_velocity = (Entity.ServerTransform.Position - m_lastPosition) / dt;
            m_angularVelocity = ksQuaternion.AngularDisplacement(m_lastRotation, Entity.ServerTransform.Rotation) / dt;
        }
        m_lastFrame = Time.Frame;
        m_lastPosition = Entity.ServerTransform.Position;
        m_lastRotation = Entity.ServerTransform.Rotation;
    }
}