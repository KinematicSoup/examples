using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;
using KSProxies.Scripts;

// Raycast gun that does client-side raycast shots and if it hits a player or physics object, sends the shoot position
// and frame timing to the server to validate.
public class RaycastGun : AmmoGun<sRaycastGun>
{
    private const int LAYER_MASK = ~(1 << Layers.LOCAL_PLAYER);

    private ksTimeKeeper m_timeKeeper;
    private float m_serverFramesPerSync = 2;
    private float m_gunOffsetY;
    private crBulletHitSpawner m_hitSpawner;
    private ceBulletTracer m_tracer;

    protected override void Start()
    {
        if (Entity == null)
        {
            return;
        }
        base.Start();
        m_tracer = Entity.GameObject.GetComponent<ceBulletTracer>();
        m_tracer.TraceOrigin = Fader.transform;
        m_timeKeeper = Entity.Room.Time.Adjuster as ksTimeKeeper;
        m_hitSpawner = Entity.Room.GameObject.GetComponent<crBulletHitSpawner>();
        m_gunOffsetY = Entity.GameObject.GetComponent<seCharacter>().GunOffsetY;
        if (m_controller != null)
        {
            m_serverFramesPerSync = GameObject.Find("Room").GetComponent<ksRoomType>().ServerFramesPerSync;
        }
    }

    protected override void LateUpdate()
    {
        ksReactor.InputManager.SetButton(Buttons.VERIFY_SHOT, false);
        base.LateUpdate();
    }

    protected override void Shoot()
    {
        RaycastHit hit;
        Vector3 direction = Camera.main.transform.forward;
        ksVector3 shootOrigin = Entity.Transform.Position + ksVector3.Up * m_gunOffsetY;
        if (Physics.Raycast(new Ray(shootOrigin, direction), out hit, ProxyAsset.Range, LAYER_MASK))
        {
            bool hitCharacter = hit.collider.GetComponentInParent<ceCharacter>() != null;
            if (hitCharacter || hit.rigidbody != null)
            {
                if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
                {
                    // Hit a client-side physics object (eg. ragdoll). Apply client-side impulse.
                    hit.rigidbody.AddForceAtPosition(direction * ProxyAsset.Impulse, hit.point, ForceMode.Impulse);
                    Entity.CallRPC(RPC.SHOOT);
                }
                else
                {
                    // Hit a dynamic object or character. The server must verify the hit. Tell the server our shoot
                    // position and frame timing.
                    ulong frame = m_timeKeeper.FrameNum;
                    // The format the time keeper stores the frame time in is not ideal. It stores it as a frame number
                    // and time offset, and the time offset is how far into the frame were are in seconds, so if a frame
                    // has a duration of .01s and the time offset is 0, we are on the previous frame, and at time .01
                    // we are on the current frame. And a frame number of max value means we are extrapolating. We
                    // need to convert to a different format here, a frame number and a t value were t:0 means we're at
                    // the frame value, and t:.8 means were are 80% of the way to the next frame.
                    float t = m_timeKeeper.TimeOffset / m_time.ServerUnscaledSyncDelta;
                    if (frame == ulong.MaxValue)
                    {
                        frame = (ulong)Math.Max(0, m_time.Frame - m_serverFramesPerSync);
                        t += 1f;
                    }
                    else
                    {
                        frame = (ulong)Math.Max(0, frame - m_serverFramesPerSync);
                    }
                    Entity.CallRPC(RPC.SHOOT, Entity.Transform.Position, frame, t);
                    ksReactor.InputManager.SetButton(Buttons.VERIFY_SHOT, true);
                }
                // If we did not hit a character, show a bullet impact.
                if (!hitCharacter)
                {
                    Vector3 rebound = Vector3.Reflect(direction, hit.normal);
                    m_hitSpawner.SpawnBulletHit(hit.point, rebound);
                }
                m_tracer.TraceBullet(hit.point, false, Color.white);
            }
            else
            {
                // We hit static terrain. Show a bullet impact.
                Entity.CallRPC(RPC.SHOOT);
                Vector3 rebound = Vector3.Reflect(direction, hit.normal);
                m_hitSpawner.SpawnBulletHit(hit.point, rebound);
                m_tracer.TraceBullet(hit.point, false, Color.white);
            }
        }
        else
        {
            // We hit nothing.
            Entity.CallRPC(RPC.SHOOT);
            m_tracer.TraceBullet(shootOrigin + direction * 100f, false, Color.white);
        }
    }

    protected override void OnShoot(ksMultiType[] args)
    {
        if (m_controller != null)
        {
            return;
        }
        base.OnShoot(args);
        if (args.Length == 0 || (args.Length == 1 && args[0]))
        {
            // Do a raycast to find the hit location and render an impact.
            Vector3 direction = Utils.GetAimDirection(Entity.Properties[Prop.AIM]);
            ksVector3 shootOrigin = Fader.transform.position;
            RaycastHit hit;
            if (Physics.Raycast(new Ray(shootOrigin, direction), out hit, ProxyAsset.Range))
            {
                m_hitSpawner.SpawnBulletHit(hit.point, hit.normal);
                if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
                {
                    // Hit a client-side physics object (eg. ragdoll). Apply client-side impulse.
                    hit.rigidbody.AddForceAtPosition(direction * ProxyAsset.Impulse, hit.point, ForceMode.Impulse);
                }
            }
        }
        else if (args.Length == 2)
        {
            // The server told us the location of the hit to render an impact at. It does this when physics objects are
            // hit.
            m_hitSpawner.SpawnBulletHit(args[0], args[1]);
        }
    }
}
