using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;
using KSProxies.Scripts;

// Projectile gun the predicively spawns a projectile that is controlled by the client locally until the server spawns
// an entity for the projectile and takes over.
public class ProjectileGun : AmmoGun<sProjectileGun>
{
    public GameObject BulletPrefab;

    private crPredictiveSpawnManager m_spawnManager;

    protected override void Start()
    {
        if (Entity == null)
        {
            return;
        }
        base.Start();
        // If the local player has this weapon, get the predictive spawn manager.
        if (m_controller != null)
        {
            m_spawnManager = Entity.Room.GameObject.GetComponent<crPredictiveSpawnManager>();
        }
    }

    protected override void Shoot()
    {
        if (BulletPrefab == null || m_spawnManager == null)
        {
            Entity.CallRPC(RPC.SHOOT, 0);
            return;
        }
        // Spawn a bullet client-side that will be taken over by the server when it spawns the entity for the bullet.
        Vector3 direction = Camera.main.transform.forward;
        Vector3 shootOrigin = Fader.transform.position;
        GameObject bulletObj = Instantiate(BulletPrefab, shootOrigin, Quaternion.identity);
        // The spawn manager gives us an id we will send to the server and the server will send back with the entity
        // it spawns for the bullet.
        byte spawnId = m_spawnManager.Add(bulletObj);
        Entity.CallRPC(RPC.SHOOT, spawnId);
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        // The predicted bullet has less velocity the higher the latency, so as to not get too far ahead of the server.
        float multiplier = ksMath.Max(.5f, 1f - Entity.Room.RPCLatency / 1000f);
        bullet.Velocity = direction * ProxyAsset.Speed * multiplier;
        if (m_controller != null && ProxyAsset.AdditiveSpeed != 0f)
        {
            ksVector3 velocity = m_controller.Velocity;
            if (!ProxyAsset.AddStrafeSpeed)
            {
                ksVector3 forward = direction;
                forward.Y = 0f;
                velocity = ksVector3.Project(velocity, forward) + new ksVector3(0f, velocity.Y, 0f);
            }
            bullet.Velocity += velocity * ProxyAsset.AdditiveSpeed;
        }
        // Set the bullet gravity multiplier to zero so the bullet has no gravity until the server takes over. This
        // prevents the appearance of double-jumping when the client applies too much gravity and the bullet needs to
        // jump upwards to sync with the server position.
        bullet.GravityMultiplier = 0f;
        bullet.Room = Entity.Room;
    }
}
