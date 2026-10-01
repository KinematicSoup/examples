using UnityEngine;
using System.Collections;

public class SimpleExplosion : MonoBehaviour
{
	void Update ()
    {
        if (!GetComponent<ParticleSystem>().IsAlive())
        {
            Destroy(gameObject);
        }
	}
}
