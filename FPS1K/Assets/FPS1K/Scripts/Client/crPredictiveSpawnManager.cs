using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Holds a map of spawn ids to client-side objects that were spawned ahead of the corresponding server entity. This is
// used for bullets to immediately show a bullet when the player shoots instead of waiting for the server to spawn a
// bullet. The spawn id is sent to the server and the server sends it back with the id of the entity it spawns, which
// is used to retrieve the game object for that entity and let the server take over control of the object.
public class crPredictiveSpawnManager : ksRoomScript
{
    // If the server does not spawn an entity for an object in this many seconds, destroy the object.
    public float Timeout = 1f;

    private byte m_nextId = 0;
    private Dictionary<byte, GameObject> m_predictiveSpawnMap =
        new Dictionary<byte, GameObject>();

    // Add a game object to the spawn map and get a spawn id for it.
    public byte Add(GameObject spawn)
    {
        Remove(m_nextId);
        m_predictiveSpawnMap[m_nextId] = spawn;
        Lifetime lifetime = spawn.AddComponent<Lifetime>();
        lifetime.Duration = Timeout;
        byte id = m_nextId;
        lifetime.OnDestroy += () => Remove(id);
        unchecked
        {
            return m_nextId++;
        }
    }

    // Remove the game object with the given spawn id from the spawn map.
    public GameObject Remove(byte id)
    {
        GameObject gameObject;
        if (m_predictiveSpawnMap.TryGetValue(id, out gameObject) && gameObject != null)
        {
            Destroy(gameObject.GetComponent<Lifetime>());
        }
        return gameObject;
    }

    // The server calls this with a spawn id to identify the client-side object on the same frame the entity is
    // spawned, if there is a client-side object for this entity.
    [ksRPC(RPC.PREDICTED_SPAWN)]
    private void PredictedSpawn(byte spawnId, uint entityId)
    {
        GameObject obj = Remove(spawnId);
        if (obj != null)
        {
            ksEntity entity = Room.GetEntity(entityId);
            if (entity == null || entity.GameObject == null)
            {
                Destroy(obj);
            }
            else
            {
                obj.transform.parent = entity.GameObject.transform;
            }
        }
    }
}