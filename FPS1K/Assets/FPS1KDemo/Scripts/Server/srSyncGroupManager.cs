using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Divides the world into a grid of sync groups that are a multiple of the tile size, and syncs all sync groups to
// players that are within the ViewDistance of that player.
public class srSyncGroupManager : ksServerRoomScript
{
    // Sync sync groups for tiles that are within this distance of the player.
    [ksEditable]
    public float ViewDistance = 160f;
    [ksEditable]
    public int TileLengthPerGroup = 2;

    private float m_cellSize;
    private int m_numRows;

    // Track a list of rigid bodies that could travel between sync groups.
    private List<ksRigidBody> m_rigidBodies = new List<ksRigidBody>();
    private srLevelGenerator m_levelGenerator;

    // Called when the script is attached.
    public override void Initialize()
    {
        m_cellSize = TileLengthPerGroup * World.TILE_SIZE;
        m_levelGenerator = Scripts.Get<srLevelGenerator>();
        if (m_levelGenerator == null)
        {
            return;
        }
        m_numRows = 1 + (m_levelGenerator.GridSize - 1) / TileLengthPerGroup;
        Room.OnSpawnEntity += OnSpawnEntity;
        Room.OnUpdate[0] += Update;
        for (int i = 0; i < Room.DynamicEntities.Count; i++)
        {
            OnSpawnEntity(Room.DynamicEntities[i]);
        }
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnSpawnEntity -= OnSpawnEntity;
        Room.OnUpdate[0] -= Update;
    }

    private void Update()
    {
        if (Time.FramesUntilSync != 0)
        {
            return;
        }
        // Iterate the rigidbodies and update their sync groups.
        for (int i = m_rigidBodies.Count - 1; i >= 0; i--)
        {
            ksRigidBody rigidBody = m_rigidBodies[i];
            // Check if the rigid body was destroyed.
            if (rigidBody.Entity == null)
            {
                m_rigidBodies[i] = m_rigidBodies[m_rigidBodies.Count - 1];
                m_rigidBodies.RemoveAt(m_rigidBodies.Count - 1);
                continue;
            }
            if (rigidBody.IsKinematic || !rigidBody.IsSleeping)
            {
                rigidBody.Entity.SyncGroup = GetSyncGroup(rigidBody.Entity);
            }
        }
        // Iterate the players and update which sync groups they can see.
        for (int i = 0; i < Room.Players.Count; i++)
        {
            ksIServerPlayer player = Room.Players[i];
            if (player.ControlledEntities.Count == 0)
            {
                continue;
            }

            //TODO: if we make the players kinematic rigidbodies again, we can remove this code.
            ksIServerEntity entity = player.ControlledEntities[0];
            entity.SyncGroup = GetSyncGroup(entity);

            if (player.IsVirtual)
            {
                continue;
            }
            ksVector2 pos = player.ControlledEntities[0].Transform.Position.XZ;
            pos += new ksVector2(World.TILE_SIZE / 2f, World.TILE_SIZE / 2f);
            // px, py are the indexes of the cell the player is in.
            int px = (int)(pos.X / m_cellSize);
            int py = (int)(pos.Y / m_cellSize);
            int x1 = (int)((pos.X - ViewDistance) / m_cellSize);
            int x2 = (int)((pos.X + ViewDistance) / m_cellSize);
            int y1 = (int)((pos.Y - ViewDistance) / m_cellSize);
            int y2 = (int)((pos.Y + ViewDistance) / m_cellSize);
            x1 = Math.Max(x1, 0);
            y1 = Math.Max(y1, 0);
            x2 = Math.Min(x2, m_numRows - 1);
            y2 = Math.Min(y2, m_numRows - 1);
            // Convert pos to be relative to the cell the player is in.
            pos.X %= m_cellSize;
            pos.Y %= m_cellSize;
            uint[] groups = player.GetSyncGroups();
            // for all tiles x1, y1 to x2, y2...
            for (int x = x1; x <= x2; x++)
            {
                int dx = x - px;
                for (int y = y1; y <= y2; y++)
                {
                    int dy = y - py;
                    // If the cell is within the view distance, add it to the synced groups.
                    if (IsCellInView(dx, dy, pos))
                    {
                        uint group = GetSyncGroup(x, y);
                        // RemoveGroup will replace the group with 0.
                        if (!RemoveGroup(group, groups))
                        {
                            //ksLog.Info("+" + group);
                            player.AddToSyncGroup(group);
                        }
                    }
                }
            }
            // Remove the player from groups it can no longer see.
            if (groups != null)
            {
                for (int j = 0; j < groups.Length; j++)
                {
                    if (groups[j] != 0)
                    {
                        //ksLog.Info("-" + groups[j]);
                        player.RemoveFromSyncGroup(groups[j]);
                    }
                }
            }
        }
    }

    // Checks if a cell is within the view distance. dx and dy and the difference between the cell indexes and the
    // the player's cell indexes, and pos is the player's position relative to the cell they are in. There is an
    // assumption that the cell is within a square centered around the player with half-extents of the view distance,
    // so if dx or dy is zero we assume it is within the view distance and do no further checks.
    private bool IsCellInView(int dx, int dy, ksVector2 pos)
    {
        if (dx == 0 || dy == 0)
        {
            return true;
        }
        ksVector2 delta = ksVector2.Zero;
        if (dx > 0)
        {
            delta.X = dx * m_cellSize - pos.X;
        }
        else if (dx < 0)
        {
            delta.X = (dx + 1) * m_cellSize - pos.X;
        }
        if (dy > 0)
        {
            delta.Y = dy * m_cellSize - pos.Y;
        }
        else if (dy < 0)
        {
            delta.Y = (dy + 1) * m_cellSize - pos.Y;
        }
        return delta.MagnitudeSquared() <= ViewDistance * ViewDistance;
    }

    // Gets the sync group for an entity based on its position.
    public uint GetSyncGroup(ksIServerEntity entity)
    {
        if (m_levelGenerator == null)
        {
            m_levelGenerator = Scripts.Get<srLevelGenerator>();
            if (m_levelGenerator == null)
            {
                return 0;
            }
        }
        ksVector2 position = entity.Transform.Position.XZ;
        position += new ksVector2(World.TILE_SIZE / 2f, World.TILE_SIZE / 2f);
        int x = (int)(position.X / m_cellSize);
        int y = (int)(position.Y / m_cellSize);
        x = ksMath.Clamp(x, 0, m_numRows - 1);
        y = ksMath.Clamp(y, 0, m_numRows - 1);
        return GetSyncGroup(x, y);
    }

    // Gets the sync group for the tile at grid indexes x and y.
    private uint GetSyncGroup(int x, int y)
    {
        return (uint)(x + y * m_numRows + 1);
    }

    // Checks if group is in groups, and replaces it with 0 if it is.
    private bool RemoveGroup(uint group, uint[] groups)
    {
        if (groups == null)
        {
            return false;
        }
        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i] == group)
            {
                groups[i] = 0;
                return true;
            }
        }
        return false;
    }

    private void OnSpawnEntity(ksIServerEntity entity)
    {
        if (entity.Scripts.Get<sePlatform>() != null || entity.Scripts.Get<sePickUp>() != null)
        {
            // Platforms and pickups don't move outside their sync groups regions so they only need their sync group
            // set once.
            entity.SyncGroup = GetSyncGroup(entity);
            return;
        }

        // Track rigid bodies that may move between sync groups.
        ksRigidBody rigidBody = entity.Scripts.Get<ksRigidBody>();
        if (rigidBody != null)
        {
            m_rigidBodies.Add(rigidBody);
            entity.SyncGroup = GetSyncGroup(entity);
        }

        // All other entities are static and don't use sync groups.
    }
}