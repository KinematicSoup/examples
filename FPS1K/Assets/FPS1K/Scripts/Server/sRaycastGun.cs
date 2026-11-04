using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor;
using KS.Reactor.Server;
using KS.Reactor.Server.PhysX;

// Raycast weapon. If the client thinks it hit a player or physics object, it sends its position and frame timing and
// the server verifies the shot. Otherwise the client just tells the server it shot, and the server tells other clients
// to render the player shooting.
[ksSharedData]
public class sRaycastGun : sAmmoGun
{
    [ksEditable]
    public int Damage = 5;
    [ksEditable]
    public int HeadshotDamage = 100;
    [ksEditable]
    public float Impulse = 5f;
    [ksEditable]
    public float Range = 200f;
    // Because the client uses input prediction, and client and server can disagree about where the client was when it
    // made the shot, so the client tell us where it shot from, and we verify that is was within this distance of the
    // server position.
    [ksEditable]
    public float PredictedPositionTolerance = 1f;

    private ksVector3 m_shootPosition;
    private ksVector2 m_shootAim;
    private ulong m_shootFrame;
    private float m_shootFrameTime;
    private bool m_verifyShot = false;
    private ksRaycastParams m_raycastArgs = new ksRaycastParams();

    public override void Equip()
    {
        m_raycastArgs.Distance = Range;
        m_raycastArgs.Filter = new ksGroupMaskFilter(Collision.SOLID | Collision.QUERY_ONLY_SHOOTABLE);
        m_raycastArgs.ExcludeEntity = Entity;
        base.Equip();
    }

    public override void OnUpdate(ksInput input)
    {
        if (input.IsPressed(Buttons.VERIFY_SHOT))
        {
            // The verify shot button is pressed on the input frame the shot was made on the client. Store the aim to
            // use for the shot.
            m_verifyShot = true;
            m_shoot = true;
            m_shootAim = m_controller.Aim;
        }
        base.OnUpdate(input);
    }

    protected override void Shoot()
    {
        if (m_verifyShot || Entity.Owner.IsVirtual)
        {
            m_verifyShot = false;
            VerifyShot();
            m_shootFrame = 0;
        }
        else
        {
            Entity.CallRPC(RPC.SHOOT);
        }
    }

    private void VerifyShot()
    {
        if (Entity.Owner.IsVirtual)
        {
            m_shootPosition = Entity.Transform.Position;
            m_shootAim = m_controller.Aim;
        }
        ksVector3 direction = Utils.GetAimDirection(m_shootAim);
        ksVector3 from = m_shootPosition + ksVector3.Up * Character.GunOffsetY;

        if (!Entity.Owner.IsVirtual)
        {
            if (m_shootFrame == 0)
            {
                return;
            }

            // Rewind objects to their positions at the point in time the client made the shot.
            //seTransformHistory.RewindAll(m_shootFrame, m_shootFrameTime);
            seTransformHistory.RewindSweep(from, from + direction * Range, m_shootFrame, m_shootFrameTime);
            // Verify that where the client says it shot from and the server position are not further apart than the
            // tolerance.
            if ((m_shootPosition - Entity.Transform.Position).MagnitudeSquared() > 
                PredictedPositionTolerance * PredictedPositionTolerance)
            {
                ksLog.Warning(this, "Player tried too shoot from outside the position tolerance. Distance: " +
                    (m_shootPosition - Entity.Transform.Position).Magnitude() + ", Tolerance: " +
                    PredictedPositionTolerance);
                // Restore object positions.
                seTransformHistory.RestoreAll();
                return;
            }
        }

        ksRaycastResult hit;
        m_raycastArgs.Origin = from;
        m_raycastArgs.Direction = direction;
        bool missed = !Entity.Room.Physics.RaycastNearest(m_raycastArgs, out hit);
        if (!Entity.Owner.IsVirtual)
        {
            // Restore object positions.
            seTransformHistory.RestoreAll();
        }
        if (missed)
        {
            // The client didn't hit anything.
            Entity.CallRPC(RPC.SHOOT);
            return;
        }

        ksIServerEntity hitEntity = (ksIServerEntity)hit.Entity;
        seCharacter character = hitEntity.Scripts.Get<seCharacter>();
        if (character != null)
        {
            // The only sphere collider on the character is the head.
            int damage = hit.Collider is ksSphereCollider ? HeadshotDamage : Damage;
            Entity.CallRPC(hitEntity.Owner, RPC.TRACE, hit.Point - hitEntity.Transform.Position);
            // False tells clients not to do a raycast and render an impact. (we don't render impacts when hitting
            // players).
            Entity.CallRPC(RPC.SHOOT, false);
            character.ApplyDamage(damage, hit.Point, Entity.Owner);
            return;
        }

        ksRigidBody rigidBody = hitEntity.Scripts.Get<ksRigidBody>();
        if (rigidBody != null && !rigidBody.IsKinematic)
        {
            // Add impulse to non-kinematic rigid bodies.
            rigidBody.AddForceAtPosition(direction * Impulse, hit.Point, ksForceMode.IMPULSE);

            // Tell clients where to render the impact when an object is hit.
            ksVector3 rebound = ksVector3.Reflect(direction, hit.Normal);
            Entity.CallRPC(RPC.SHOOT, hit.Point, rebound);
        }
        else
        {
            Entity.CallRPC(RPC.SHOOT);
        }
    }

    protected override void OnRequestShoot(ksMultiType[] args)
    {
        if (args.Length == 3)
        {
            // The client sends the position they shot from and the frame timing to use for shot verification.
            m_shootPosition = args[0];
            m_shootFrame = args[1];
            m_shootFrameTime = args[2];
            // Wait until we receive the verify shot input to shoot so we use the same aim input as the client.
            m_shoot = false;
        }
    }
}