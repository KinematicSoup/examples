using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// The head controller moves the collider for the head based on the player's aim and rotation to keep it aligned with
// what the client renders.
public class seHeadController : ksServerEntityScript
{
    // The head rotates around this point in local space.
    [ksEditable]
    public ksVector3 Pivot = new ksVector3(0f, .98f, 0f);
    // The head is this far away from the pivot.
    [ksEditable]
    public float Distance = .62f;
    // How much the aim pitch affects the head.
    [ksEditable]
    public float PitchWeight = .66f;
    // How much the aim yaw affects the head.
    [ksEditable]
    public float YawWeight = .33f;
    
    private ksSphereCollider m_head;

    private FPSController m_controller;

    private static ksRange m_degrees = new ksRange(-180f, 180f);

    // Called when the script is attached.
    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;
        m_head = Scripts.Get<ksSphereCollider>();
    }

    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }

    public void Update()
    {
        if (m_controller == null)
        {
            m_controller = Entity.PlayerController as FPSController;
            if (m_controller == null)
            {
                return;
            }
        }
        if (!m_controller.HeadPositionStale)
        {
            return;
        }
        m_controller.HeadPositionStale = false;

        ksVector2 aim = m_controller.Aim;
        ksVector3 offset = new ksVector3(0f, Distance, 0f);
        if (aim.X != 0f)
        {
            ksVector3 euler = ksVector3.Zero;
            euler.X = -aim.X * PitchWeight;

            float yaw = 90f - aim.Y;
            float facing = Transform.Forward().XZ.ToDegrees();
            if (yaw != facing)
            {
                euler.Y = -m_degrees.Wrap(yaw - facing) * YawWeight;
            }

            offset *= ksQuaternion.FromEuler(euler);
        }
        m_head.Offset = Pivot + offset;
    }
}