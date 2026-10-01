using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Used for entities such as bullets that have a game object spawned on the client before the server spawns a
// corresponding entity to interpolate the client game object from the client position to the server position.
public class cePredictiveSpawnInterpolator : ksEntityScript
{
    // The prefab to spawn as a child if there was no client-side game object spawned for this entity.
    public GameObject Prefab;
    // The time in seconds it takes to interpolate from the client position to the server position.
    public float Duration = .5f;

    private float m_timer;
    private Vector3 m_delta;

    // Called after properties are initialized.
    public override void Initialize()
    {
        m_timer = Duration;
    }

    // Called every frame.
    private void Update()
    {
        Transform child;
        if (transform.childCount == 0f)
        {
            // The game object has no child, which means there was no client-side object spawned for this entity, so
            // we will create one at the server position and destroy this script.
            child = Instantiate(Prefab, transform).transform;
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            Destroy(this);
            return;
        }
        // The child is the client-side game object.
        child = transform.GetChild(0);
        m_timer -= Time.UnscaledDelta;
        if (m_timer <= 0f)
        {
            // The client object has reached the server position and no longer needs to be interpolated.
            child.localPosition = Vector3.zero;
            Destroy(this);
            return;
        }
        if (m_delta == Vector3.zero)
        {
            // Calculate the initial delta between the client and server position.
            m_delta = child.localPosition;
            if (m_delta == Vector3.zero)
            {
                // If the client object is already at the server position, we can destroy this script.
                Destroy(this);
                return;
            }
        }
        // Interpolate the local position towards zero using cosine (ease-in ease-out).
        float t = .5f - ksMath.Cos((m_timer / Duration) * ksMath.PI) / 2f;
        child.localPosition = m_delta * t;
    }

    // The server calls this with a spawn id to identify the client-side object on the same frame the entity is
    // spawned, if there is a client-side object for this entity.
    [ksRPC(RPC.PREDICTED_SPAWN)]
    private void PredictedSpawn(byte id)
    {
        crPredictiveSpawnManager spawnManager = Room.GameObject.GetComponent<crPredictiveSpawnManager>();
        if (spawnManager != null)
        {
            GameObject obj = spawnManager.Remove(id);
            if (obj != null)
            {
                obj.transform.parent = transform;
            }
        }
    }
}