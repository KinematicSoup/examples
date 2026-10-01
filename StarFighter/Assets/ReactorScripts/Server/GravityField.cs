using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using KS.Reactor.Server;
using KS.Reactor;

/**
 * Enables gravity on rigid bodies that overlap this entity, and disables their gravity when they stop overlapping.
 */
public class GravityField : ksServerEntityScript
{
    // Called after all other scripts on all entities are attached.
    public override void Initialize()
    {
        Entity.OnOverlapStart += OverlapStart;
        Entity.OnOverlapEnd += OverlapEnd;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Entity.OnOverlapStart -= OverlapStart;
        Entity.OnOverlapEnd -= OverlapEnd;
    }

    private void OverlapStart(ksOverlap overlap)
    {
        ksRigidBody rigidBody = overlap.Entity1.Scripts.Get<ksRigidBody>();
        if (rigidBody != null)
        {
            rigidBody.UseGravity = true;
        }
    }

    private void OverlapEnd(ksOverlap overlap)
    {
        ksRigidBody rigidBody = overlap.Entity1.Scripts.Get<ksRigidBody>();
        if (rigidBody != null)
        {
            rigidBody.UseGravity = false;
        }
    }
}