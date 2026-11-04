using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Stores a history of sync frame positions and rotations for the entity which can be used to revert the entity to a
// point in the passed for validating client shots at the time the client made the shot.
public class seTransformHistory : ksServerEntityScript
{
    // The number of sync frames of history to keep.
    public const int HISTORY_SIZE = 30;

    private struct Frame
    {
        public ksVector3 Position;
        public ksQuaternion Rotation;

        public Frame(ksVector3 position, ksQuaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    private Frame[] m_history = new Frame[HISTORY_SIZE];
    private ksRigidBody m_rigidBody;
    // Index in history to use for the next frame.
    private int m_next = 0;
    // Number of frames in history.
    private int m_count = 0;
    // Number of consecuitive sync frames the rigid body is asleep.
    private int m_sleepCount = 0;
    // Number of sync frames since the entity was destroyed.
    private int m_destroyCount = 0;
    // Index of this script in the objects list.
    private int m_index = -1;
    // Before rewinding to an earlier point in history, the current transform is stored here.
    private Frame m_current;
    private bool m_storedCurrent = false;
    private bool m_disabledColliders = false;
    // True if the object did not move at any point in the history.
    private bool m_unmoved = false;
    private List<ksCollider> m_colliders;
    // We store a reference to the entity so we can keep the entity reference when the entity is destroyed.
    private ksIServerEntity m_entity;
    // One the entity is destroyed, if we rewind to a point before the entity was destroyed, we create a temp entity
    // with the same colliders.
    private ksIServerEntity m_tempEntity;
    // We store the scale because we cannot read it from the transform after the entity is destroyed and we need to
    // know what scale to create the temp entity with.
    private ksVector3 m_scale;

    private new ksIServerEntity Entity
    {
        get { return m_entity; }
    }

    private static new ksIServerRoom Room
    {
        get { return m_room; }
    }

    private static new ksServerTime Time
    {
        get { return m_room.Time; }
    }

    // Last frame number stored in history.
    private static ulong m_lastFrame;
    private static ksIServerRoom m_room;
    private static int m_framesPerSync = 0;
    private static List<seTransformHistory> m_objects = new List<seTransformHistory>();
    private static ksSweepParams m_sweepArgs = new ksSweepParams();

    // Called when the script is attached.
    public override void Initialize()
    {
        if (m_room == null)
        {
            m_room = base.Room;
            m_sweepArgs.Shape = new ksSphere(1f);
            m_sweepArgs.Flags &= ~ksQueryFlags.EXCLUDE_OVERLAPS;
            m_sweepArgs.Filter = new ksGroupMaskFilter(
                Collision.PLAYER | Collision.DYNAMIC | Collision.MOVING);
            Room.OnUpdate[0] += UpdateFramesPerSync;
        }
        m_entity = base.Entity;
        m_scale = Transform.Scale;
        m_index = m_objects.Count;
        m_objects.Add(this);
        m_rigidBody = Scripts.Get<ksRigidBody>();
        Room.OnUpdate[10] += Update;
        Entity.OnDestroy += OnDestroy;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        // Don't remove the Update function; we want to continue updating until the entity has been destroyed for every
        // sync frame in the history.
        Entity.OnDestroy -= OnDestroy;
    }

    private void OnDestroy()
    {
        // Get all the colliders so we can attach them to the temp entity we create if we need to rewind to before the
        // entity was destroyed.
        if (m_colliders == null)
        {
            m_colliders = Scripts.GetAll<ksCollider>();
        }
    }
    
    // Called during the update cycle
    private void Update()
    {
        if (Time.FramesUntilSync != 0)
        {
            return;
        }
        if (Entity.IsDestroyed)
        {
            m_destroyCount++;
            if (m_destroyCount >= HISTORY_SIZE)
            {
                // The entity was destroyed for every sync frame in the history, so we no longer need to track it.
                Room.OnUpdate[10] -= Update;
                if (m_index != m_objects.Count - 1)
                {
                    seTransformHistory last = m_objects[m_objects.Count - 1];
                    last.m_index = m_index;
                    m_objects[m_index] = last;
                }
                m_objects.RemoveAt(m_objects.Count - 1);
            }
            return;
        }
        StoreHistory(m_next);
        m_lastFrame = Time.Frame;
        m_next++;
        m_next %= HISTORY_SIZE;
        if (m_count < HISTORY_SIZE)
        {
            m_count++;
        }
        if (m_rigidBody != null && !m_rigidBody.IsKinematic && m_rigidBody.IsSleeping)
        {
            m_sleepCount++;
            if (m_sleepCount >= HISTORY_SIZE)
            {
                // The object was asleep for every sync frame in the history. We can stop updating until it wakes up.
                m_unmoved = true;
                m_sleepCount = 0;
                Room.OnUpdate[10] -= Update;
                Entity.OnWake += Wake;
            }
        }
        else
        {
            m_sleepCount = 0;
        }
    }

    // Stores transform history at the given index.
    protected virtual void StoreHistory(int index)
    {
        m_history[index] = new Frame(Transform.Position, Transform.Rotation);
    }

    // Stores the current transform.
    protected virtual void StoreCurrent()
    {
        m_current.Position = Transform.Position;
        m_current.Rotation = Transform.Rotation;
    }

    // Restores the transform values that were stored using StoreCurrent.
    protected virtual void RestoreCurrent()
    {
        Transform.Position = m_current.Position;
        Transform.Rotation = m_current.Rotation;
    }

    // Creates a temp entity with the same colliders
    protected virtual ksIServerEntity CreateTempEntity()
    {
        ksIServerEntity tempEntity = Room.SpawnEntity();
        // Prevent temp entity from syncing by setting the sync group to a group that syncs to no clients.
        tempEntity.SyncGroup = ksFixedDataWriter.ENCODE_4BYTE;
        tempEntity.Transform.Scale = m_scale;
        tempEntity.CollisionFilter = Entity.CollisionFilter;
        foreach (ksCollider collider in m_colliders)
        {
            tempEntity.Scripts.Attach(collider);
        }
        return tempEntity;
    }

    // Sets the transform to the values from history at the given index, interpolated by t into the next index.
    protected virtual void RewindTo(ksTransform transform, int index, float t)
    {
        Frame frame = m_history[index];
        if (t > 0)
        {
            // Extrapolate at most 2 frames passed the next frame.
            t = Math.Min(t, 3f);
            index++;
            index %= HISTORY_SIZE;
            Frame next = m_history[index];
            frame.Position += (next.Position - frame.Position) * t;
            frame.Rotation = ksQuaternion.Lerp(frame.Rotation, next.Rotation, t);
        }
        transform.Position = frame.Position;
        transform.Rotation = frame.Rotation;
    }

    private void Wake()
    {
        m_unmoved = false;
        Entity.OnWake -= Wake;
        Room.OnUpdate[10] += Update;
    }

    private void UpdateFramesPerSync()
    {
        // The number of frames per sync is not in the API currently so we have to calculate it...
        if (m_framesPerSync >= Time.FramesUntilSync + 1)
        {
            Room.OnUpdate[0] -= UpdateFramesPerSync;
        }
        else
        {
            m_framesPerSync = (int)Room.Time.FramesUntilSync + 1;
        }
    }

    // Rewinds to a point in the history. Count is the number of frames to rewind, t is how far into the next frame to
    // interpolate (beyond 1 to extrapolate).
    public void Rewind(int count, float t = 0f)
    {
        if (m_unmoved || count > HISTORY_SIZE)
        {
            return;
        }
        ksTransform transform;
        if (Entity.IsDestroyed)
        {
            count -= m_destroyCount;
            if (count <= 0 || count > m_count)
            {
                // The entity was destroyed at the point in time we are rewinding to.
                if (m_tempEntity != null)
                {
                    m_tempEntity.Destroy();
                    m_tempEntity = null;
                }
                return;
            }
            // The entity is destroyed now, but was not at the point in time we are rewinding to, so we create a temp
            // entity with the same colliders.
            if (m_tempEntity == null)
            {
                m_tempEntity = CreateTempEntity();
            }
            transform = m_tempEntity.Transform;
        }
        else
        {
            if (count > m_count)
            {
                // The entity did not exist at the time we are rewinding to, so we disable all colliders on the entity.
                if (!m_disabledColliders)
                {
                    m_disabledColliders = true;
                    if (m_colliders == null)
                    {
                        m_colliders = Scripts.GetAll<ksCollider>();
                    }
                    foreach (ksCollider collider in m_colliders)
                    {
                        collider.IsEnabled = false;
                    }
                }
                return;
            }
            // Store the current location if we haven't already so we can restore it later.
            if (!m_storedCurrent)
            {
                m_storedCurrent = true;
                StoreCurrent();
            }
            transform = Transform;
        }
        int index = m_next - count;
        if (index < 0)
        {
            index += HISTORY_SIZE;
        }
        RewindTo(transform, index, t);
    }

    // Restores the entity to the current location.
    public void Restore()
    {
        if (Entity.IsDestroyed)
        {
            // If we rewound to a point before the entity was destroyed and created a temp entity, destroy the temp
            // entity.
            if (m_tempEntity != null)
            {
                m_tempEntity.Destroy();
                m_tempEntity = null;
            }
        }
        else if (m_storedCurrent)
        {
            m_storedCurrent = false;
            Transform.Position = m_current.Position;
            Transform.Rotation = m_current.Rotation;
        }
        else if (m_disabledColliders)
        {
            // If we rewound to a point before the entity was spawned and disabled the colliders, reenable the
            // colliders.
            m_disabledColliders = true;
            for (int i = 0; i < m_colliders.Count; i++)
            {
                m_colliders[i].IsEnabled = true;
            }
        }
    }

    // Does a sweep and rewinds entities the sweep finds to a specific frame (and subframe if t is non-zero) in history.
    // Used to rewind objects near the path of a raycast shot to validate the shot.
    public static void RewindSweep(ksVector3 from, ksVector3 to, ulong frame, float t = 0f, float radius = 1.5f)
    {
        if (m_framesPerSync == 0)
        {
            return;
        }
        int count = 1 + (int)(m_lastFrame - frame) / m_framesPerSync;
        if (count > HISTORY_SIZE)
        {
            ksLog.Warning("Cannot rewind " + count + " sync frames. History size: " + HISTORY_SIZE);
            count = HISTORY_SIZE;
            t = 0f;
        }

        if (to == from)
        {
            return;
        }
        ksSphere sphere = (ksSphere)m_sweepArgs.Shape;
        sphere.Radius = radius;
        m_sweepArgs.Origin = from;
        m_sweepArgs.End = to;
        int num = 0;
        foreach (ksSweepResult hit in Room.Physics.Sweep(m_sweepArgs))
        {
            ksIServerEntity entity = (ksIServerEntity)hit.Entity;
            seTransformHistory history = entity.Scripts.Get<seTransformHistory>();
            if (history != null)
            {
                num++;
                history.Rewind(count, t);
            }
        }
        Room.Physics.SyncTransforms();
    }

    // Rewind all tracked entities to a specific frame (and subframe if t is non-zero) in history.
    // (This is unused because it's too slow when there's lots of objects. Use RewindSweep instead)
    public static void RewindAll(ulong frame, float t = 0f)
    {
        if (m_framesPerSync == 0)
        {
            return;
        }
        int count = 1 + (int)(m_lastFrame - frame) / m_framesPerSync;
        if (count > HISTORY_SIZE)
        {
            ksLog.Warning("Cannot rewind " + count + " sync frames. History size: " + HISTORY_SIZE);
            count = HISTORY_SIZE;
            t = 0f;
        }
        for (int i = 0; i < m_objects.Count; i++)
        {
            m_objects[i].Rewind(count, t);
        }
    }

    // Restore all tracked entities to their current locations.
    public static void RestoreAll()
    {
        for (int i = 0; i < m_objects.Count; i++)
        {
            m_objects[i].Restore();
        }
        Room.Physics.SyncTransforms();
    }
}