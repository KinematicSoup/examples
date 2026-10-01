using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;
using KS.Reactor.Client.Unity;

// Controls the movement of a predictively-spawned bullet client-side before the entity for the bullet is synced and
// the server takes over.
public class Bullet : MonoBehaviour
{
    public Vector3 Velocity
    {
        get { return m_velocity; }
        set { m_velocity = value; }
    }
    private Vector3 m_velocity;

    public float GravityMultiplier
    {
        get { return m_gravityMultiplier; }
        set { m_gravityMultiplier = value; }
    }
    private float m_gravityMultiplier;

    public ksRoom Room
    {
        get { return m_room; }
        set { m_room = value; }
    }
    private ksRoom m_room;

    // Update is called once per frame
    private void Update()
    {
        if (transform.parent != null)
        {
            // The bullet will have a parent once the entity for the bullet is spawned, in which case the client no
            // longer needs this script to control the bullet.
            Destroy(this);
            return;
        }
        transform.position += m_velocity * m_room.Time.AdjustedDelta;
        m_velocity += m_room.Physics.Gravity * m_gravityMultiplier * m_room.Time.AdjustedDelta;
    }
}
