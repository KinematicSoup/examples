using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Spawns bullet impacts and explosiions
public class crBulletHitSpawner : ksRoomScript
{
    public GameObject ShotPrefab;
    public GameObject ExplosionPrefab;

    private GameObjectPool m_shotPool;
    private GameObjectPool m_explosionPool;

    public override void Initialize()
    {
        // Create object pools
        GameObject poolObj = new GameObject("Shot Pool");
        m_shotPool = poolObj.AddComponent<GameObjectPool>();
        m_shotPool.Template = ShotPrefab;

        poolObj = new GameObject("Explosion Pool");
        m_explosionPool = poolObj.AddComponent<GameObjectPool>();
        m_explosionPool.Template = ExplosionPrefab;
    }

    public override void Detached()
    {
        if (m_shotPool != null)
        {
            Destroy(m_shotPool);
        }
        if (m_explosionPool != null)
        {
            Destroy(m_explosionPool);
        }
    }

    public void SpawnBulletHit(Vector3 position, Vector3 direction)
    {
        // Only spawn bullet impacts that are nearby or in the field of view.
        if (!VisibilityUtils.IsNearOrVisible(position))
        {
            return;
        }
        Transform t = m_shotPool.Fetch().transform;
        t.position = position;
        t.forward = direction;
        t.GetComponent<Lifetime>().Pool = m_shotPool;
        Fader fader = t.GetComponentInChildren<Fader>();
        fader.Alpha = fader.InitialAlpha;
    }

    [ksRPC(RPC.EXPLOSION)]
    private void SpawnExplosion(Vector3 position, float radius)
    {
        // Only spawn explosions that are nearby or in the field of view.
        if (!VisibilityUtils.IsNearOrVisible(position, 40f, 160f))
        {
            return;
        }
        Transform t = m_explosionPool.Fetch().transform;
        t.position = position;
        t.localScale = Vector3.one * radius * 2f;
        t.GetComponent<Lifetime>().Pool = m_explosionPool;
        Fader fader = t.GetComponentInChildren<Fader>();
        fader.Alpha = fader.InitialAlpha;
    }
}