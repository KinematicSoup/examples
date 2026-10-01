using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Creates line tracers for bullets shot by the entity this is attached to.
public class ceBulletTracer : ksEntityScript
{
    public GameObject TracePrefab;

    // Where the bullets are shot from.
    public Transform TraceOrigin
    {
        get { return m_traceOrigin; }
        set { m_traceOrigin = value; }
    }
    private Transform m_traceOrigin;

    // Renders a red tracer from the TraceOrigin to the impact point relative to the local player. The server calls
    // this RPC when the local player is hit.
    [ksRPC(RPC.TRACE)]
    public void TraceBullet(ksVector3 impactPoint)
    {
        TraceBullet(impactPoint, true, Color.red);
    }

    // Renders a tracer from the TraceOrigin to the impact point. If localSpace is true, the impact point is relative
    // to the local player's position.
    public void TraceBullet(ksVector3 impactPoint, bool localSpace, Color color)
    {
        if (m_traceOrigin != null && TracePrefab != null)
        {
            LineRenderer line = Instantiate(TracePrefab).GetComponent<LineRenderer>();
            if (line != null)
            {
                if (localSpace)
                {
                    if (Room.OwnedEntities.Count == 0)
                    {
                        return;
                    }
                    impactPoint += Room.OwnedEntities[0].Transform.Position;
                }
                color.a = line.startColor.a;
                line.startColor = color;
                color.a = line.endColor.a;
                line.endColor = color;
                line.transform.position = m_traceOrigin.position;
                Vector3[] positions = new Vector3[2];
                positions[0] = impactPoint - m_traceOrigin.position;
                positions[1] = Vector3.zero;
                line.SetPositions(positions);
            }
        }
    }
}