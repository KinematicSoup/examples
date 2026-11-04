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
    // How much the aim pitch affects the head when looking down.
    [ksEditable]
    public float PitchDownWeight = .75f;
    // How much the aim pitch affects the head when looking up.
    [ksEditable]
    public float PitchUpWeight = .66f;
    // The head is offset from the pivot by this much when the aim pitch is 0.
    [ksEditable]
    public float NeutralPitch = 5f;
    // How much the head move side to side when the pitch reaches 90 degrees.
    [ksEditable]
    public float PitchSideWeight = 0.06f;
    // How much the head moves side to side when the yaw reaches 90 degrees as a function of the pitch.
    [ksEditable]
    public ksCurve YawSideWeightByPitch;

    private ksSphereCollider m_head;

    private FPSController m_controller;

    private static ksRange m_degrees = new ksRange(-180f, 180f);

    // Called when the script is attached.
    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;
        m_head = Scripts.Get<ksSphereCollider>();
        YawSideWeightByPitch.PreWrapMode = ksCurve.WrapModes.First;
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
        ksVector3 euler = ksVector3.Zero;

        // Apply pitch to the head offset.
        euler.X = -aim.X * (aim.X > 0 ? PitchUpWeight : PitchDownWeight) + NeutralPitch;
        offset *= ksQuaternion.FromEuler(euler);

        // Head moves side to side based on the pitch and yaw.
        offset.X += aim.X * PitchSideWeight / 90f;
        float yaw = 90f - aim.Y;
        float facing = Transform.Forward().XZ.ToDegrees();
        if (yaw != facing)
        {
            yaw = m_degrees.Wrap(yaw - facing);

            // yaw side weight changes based on the pitch.
            offset.X -= yaw * YawSideWeightByPitch.Evaluate(-aim.X) / 90f;
        }

        m_head.Offset = Pivot + offset;
    }
}
