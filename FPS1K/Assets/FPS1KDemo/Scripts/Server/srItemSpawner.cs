using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Spawns items. There are a given number of items per tile at the start, and a new item is spawned everytime an item
// is collected.
public class srItemSpawner : ksServerRoomScript
{
    public struct ItemInfo
    {
        // Item entity name
        public string Name;
        // How likely the item is to spawn.
        public float Weight;
    }

    // Items that can spawn
    [ksEditable]
    public ItemInfo[] Items;

    // Number of items to spawn per tile in the level grid.
    [ksEditable]
    public float ItemsPerTile = .5f;

    private float m_totalWeight;
    private srPlayerSpawner m_spawner;
    private srLevelGenerator m_generator;
    private srSyncGroupManager m_syncGroupManager;

    // Called when the script is attached.
    public override void Initialize()
    {
        if (Items == null)
        {
            return;
        }
        foreach (ItemInfo item in Items)
        {
            m_totalWeight += item.Weight;
        }
        m_spawner = Scripts.Get<srPlayerSpawner>();
        m_generator = Scripts.Get<srLevelGenerator>();
        m_syncGroupManager = Scripts.Get<srSyncGroupManager>();
        if (m_generator != null)
        {
            m_generator.OnGenerateLevel += SpawnItems;
        }
    }

    // Called when the script is detached.
    public override void Detached()
    {
        if (m_generator != null)
        {
            m_generator.OnGenerateLevel -= SpawnItems;
        }
    }

    private void SpawnItems()
    {
        int count = (int)Math.Ceiling(m_generator.NumTiles * ItemsPerTile);
        for (int i = 0; i < count; i++)
        {
            SpawnItem();
        }
    }

    public void SpawnItem()
    {
        if (m_spawner == null || m_totalWeight == 0f)
        {
            return;
        }
        float num = m_spawner.Rand.NextFloat() * m_totalWeight;
        string name = Items[Items.Length - 1].Name;
        foreach (ItemInfo item in Items)
        {
            if (num < item.Weight)
            {
                name = item.Name;
                break;
            }
            num -= item.Weight;
        }
        ksIServerEntity entity = Room.SpawnEntity(name);
        if (entity == null)
        {
            return;
        }
        entity.Transform.Position = m_spawner.GetSpawnPoint(entity);
        if (m_syncGroupManager != null)
        {
            entity.SyncGroup = m_syncGroupManager.GetSyncGroup(entity);
        }
        sePickUp pickUp = entity.Scripts.Get<sePickUp>();
        if (pickUp != null)
        {
            // Spawn a new item when the item is collected.
            pickUp.OnPickUp += SpawnItem;
        }
    }
}