using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public struct Waypoint
{
    // Position relative to the entity's starting position and rotation.
    public ksVector3 Position;
    // Delay in seconds before moving to the next way point.
    public float Delay;
}

// Moves an entity between a series of waypoints.
public class sePlatform : ksServerEntityScript
{
    // Waypoints to move between, relative to the entity's starting position and rotation.
    [ksEditable]
    public Waypoint[] Waypoints;

    [ksEditable]
    public float Speed = 1.0f;

    [ksEditable]
    public bool Loop = false;

    [ksEditable]
    public int WaypointIndex = 0;

    private ksVector3 m_origin;
    private bool m_incrementWP = true;
    private float m_delay = 0.0f;
    private Waypoint m_lastWP;
    private Waypoint m_nextWP;
    private int m_startIndex;

    // The velocity of the entity, used to get the ground velocity for players standing on moving platforms.
    public ksVector3 Velocity
    {
        get { return m_velocity; }
    }
    private ksVector3 m_velocity;

    private ksRigidBody m_rigidBody;

    /**
     * Called when the script is attached.
     */
    public override void Initialize()
    {
        m_origin = Transform.Position;
        m_startIndex = WaypointIndex;
        srLevelGenerator levelGenerator = Room.Scripts.Get<srLevelGenerator>();
        if (levelGenerator != null && levelGenerator.IncludeDynamicRigidbodies)
        {
            m_rigidBody = Scripts.Get<ksRigidBody>();
        }
        Reset();
        Room.OnUpdate[1] += Update;
    }

    /**
     * Called when the script is detached.
     */
    public override void Detached()
    {
        Room.OnUpdate[1] -= Update;
    }

    /**
     * Called after the room simulation step.
     */
    private void Update()
    {
        if (Time.Delta == 0f)
        {
            return;
        }
        if (m_delay > 0)
        {
            m_delay -= Time.Delta;
            m_velocity = ksVector3.Zero;
        }
        else
        {
            ksVector3 target = GetWorldPosition(m_nextWP.Position);
            ksVector3 position = ksVector3.MoveTowards(Entity.Transform.Position, target, Speed * Time.Delta);
            if (m_rigidBody == null)
            {
                m_velocity = (position - Transform.Position) / Time.Delta;
                Transform.Position = position;
            }
            else
            {
                m_rigidBody.KinematicMovement = position - Transform.Position;
                m_velocity = m_rigidBody.KinematicMovement / Time.Delta;
            }
            if (position == target)
            {
                m_delay = m_nextWP.Delay;
                m_nextWP = NextWP();
            }
        }
    }

    public void Reset()
    {
        WaypointIndex = m_startIndex;
        if (Waypoints.Length > WaypointIndex)
        {
            m_incrementWP = true;
            m_lastWP = Waypoints[WaypointIndex];
            m_nextWP = NextWP();
            Entity.Transform.Teleport(GetWorldPosition(m_lastWP.Position));
            m_delay = m_lastWP.Delay;
        }
    }

    private Waypoint NextWP()
    {
        if (!Loop)
        {
            if (m_incrementWP && WaypointIndex == Waypoints.Length - 1)
            {
                m_incrementWP = false;
            }
            else if (!m_incrementWP && WaypointIndex == 0)
            {
                m_incrementWP = true;
            }
        }
        WaypointIndex += m_incrementWP ? 1 : -1;
        WaypointIndex %= Waypoints.Length;
        return Waypoints[WaypointIndex];
    }

    private ksVector3 GetWorldPosition(ksVector3 position)
    {
        position *= Transform.Rotation;
        position += m_origin;
        return position;
    }
}