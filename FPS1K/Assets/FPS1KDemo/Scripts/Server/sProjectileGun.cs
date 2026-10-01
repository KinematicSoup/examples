using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor;
using KS.Reactor.Server;

// Gun that fires projectile entities with a velocity. The shooting client predicts and spawns the projectiles ahead of
// the server.
[ksSharedData]
public class sProjectileGun : sAmmoGun
{
    [ksEditable]
    public string BulletPrefab = "Bullet";
    [ksEditable]
    public float Speed = 25f;
    // The player's speed multiplied by this is added to the projectile's speed.
    [ksEditable]
    public float AdditiveSpeed = 1f;
    // If false, only the players forward and vertical speed is added to the projectile's speed.
    [ksEditable]
    public bool AddStrafeSpeed = false;
    // Mutiplier for the projectile's gravity.
    [ksEditable]
    public float GravityMultiplier = 0f;
    [ksEditable]
    public int Damage = 30;
    [ksEditable]
    public float Impulse = 10f;
    // The projectile spawns this far in front of the player.
    [ksEditable]
    public float ShootOffset = .25f;

    // The id used for the player who shot the projectile to identify it.
    private byte m_spawnId;

    protected override void Shoot()
    {
        Entity.CallRPC(RPC.SHOOT);
        ksVector3 direction = Utils.GetAimDirection(m_controller.Aim);
        ksVector3 position = Entity.Transform.Position + ksVector3.Up * Character.GunOffsetY + direction * ShootOffset;
        seBullet bullet = Entity.Room.SpawnEntity(BulletPrefab, position).Scripts.Get<seBullet>();
        if (bullet != null)
        {
            bullet.Velocity = direction * Speed;
            if (m_controller != null && AdditiveSpeed != 0f)
            {
                ksVector3 velocity = m_controller.Velocity;
                if (!AddStrafeSpeed)
                {
                    ksVector3 forward = direction;
                    forward.Y = 0f;
                    velocity = ksVector3.Project(velocity, forward) + new ksVector3(0f, velocity.Y, 0f);
                }
                //ksLog.Info(m_controller.Velocity + " -> " + velocity);
                bullet.Velocity += velocity * AdditiveSpeed;
            }
            bullet.GravityMultiplier = GravityMultiplier;
            bullet.Damage = Damage;
            bullet.Impulse = Impulse;
            bullet.Owner = Entity;
            // Tell the player who shot the projectile that this is the entity for the projectile.
            Entity.Room.CallRPC(Entity.Owner, RPC.PREDICTED_SPAWN, m_spawnId, bullet.Entity.Id);
        }
    }

    protected override void OnRequestShoot(ksMultiType[] args)
    {
        m_spawnId = args[0];
    }
}