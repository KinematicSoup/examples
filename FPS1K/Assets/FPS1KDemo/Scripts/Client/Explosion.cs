using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;

// Plays an explosion sound and applies an explosive force to client-side physics objects (dead-player ragdolls).
public class Explosion : MonoBehaviour
{
    public LayerMask layerMask;
    public float Impulse = 5f;
    public Sound ExplodeSound;
    private bool m_applyExplosiveForce = false;

    public void OnEnable()
    {
        SoundManager.Instance.Play(ExplodeSound, gameObject);
        // If we try to look for ragdolls to apply explosive force here, we may miss some that just died, so wait until
        // late update.
        m_applyExplosiveForce = true;
    }

    private void LateUpdate()
    {
        if (!m_applyExplosiveForce)
        {
            return;
        }
        m_applyExplosiveForce = false;
        float radius = .5f * transform.localScale.x;
        foreach (Collider collider in Physics.OverlapSphere(transform.position, radius, layerMask))
        {
            Rigidbody rigidBody = collider.GetComponent<Rigidbody>();
            if (rigidBody != null && !rigidBody.isKinematic)
            {
                rigidBody.AddExplosionForce(Impulse, transform.position, radius, .1f, ForceMode.Impulse);
            }
        }
    }
}
