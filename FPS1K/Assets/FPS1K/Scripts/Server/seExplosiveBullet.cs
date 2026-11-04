using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// A bullet that explodes on impact, adding an explosive impulse to nearby physics objects and damaging nearby players.
public class seExplosiveBullet : seBullet
{
    [ksEditable]
    public float ExplosionRadius = 1f;

    protected override bool Hit(ksSweepResult hit)
    {
        Room.CallRPC(RPC.EXPLOSION, hit.Point, ExplosionRadius);
        srLevelGenerator levelGenerator = Room.Scripts.Get<srLevelGenerator>();
        if (levelGenerator != null && levelGenerator.IncludeDynamicRigidbodies)
        {
            Physics.ApplyExplosiveForce(hit.Point, ExplosionRadius, Impulse, upwardsModifier: .1f,
                groupMask: Collision.DYNAMIC);
        }
        // Find nearby players and apply damage.
        ksOverlapParams args = new ksOverlapParams(new ksSphere(ExplosionRadius), hit.Point, ksQuaternion.Identity,
            new ksGroupMaskFilter(Collision.PLAYER));
        foreach (ksOverlapResult overlap in Physics.Overlap(args))
        {
            ksIServerEntity entity = (ksIServerEntity)overlap.Entity;
            seCharacter character = entity.Scripts.Get<seCharacter>();
            if (character != null)
            {
                ksVector3 closestPoint = Physics.GetClosestPoint(hit.Point, overlap.Collider);
                float distance = (Physics.GetClosestPoint(hit.Point, overlap.Collider) - hit.Point).Magnitude();
                int damage = Math.Max(0, (int)Math.Ceiling(Damage * (1f - distance / ExplosionRadius)));
                if (entity == Owner)
                {
                    // Do half-damage to the player who shot the bullet.
                    damage /= 2;
                }
                character.ApplyDamage(damage, closestPoint, Owner.Owner);
                if (damage >= 1)
                {
                    //float mass = entity.Scripts.Get<ksRigidBody>().Mass;
                    //if (mass <= 0f)
                    //{
                    //    mass = 1f;
                    //}
                    float mass = 5f;
                    ksVector3 origin = hit.Point - ksVector3.Up * .1f - Velocity.Normalized() * .1f;
                    ksVector3 impulse = (closestPoint - origin).Normalized() * (Impulse / mass) *
                        (1f - distance / ExplosionRadius);
                    FPSController controller = entity.PlayerController as FPSController;
                    // Add an impulse to the player and stun them until they touch the ground.
                    if (controller != null)
                    {
                        controller.Velocity += impulse;
                        controller.Stunned = true;
                    }
                }
            }
        }
        // Return true to destroy the bullet.
        return true;
    }
}