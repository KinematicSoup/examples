using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Destroys the object it is attached to and fires an event after a set period of time.
public class Lifetime : MonoBehaviour
{
    // Time in seconds to destroy the object.
    public float Duration = 1f;

    // Called when the timer reaches zero.
    public event Action OnDestroy;

    // If not null, the object will be returned to this pool instead of destroyed.
    public GameObjectPool Pool;

    private float m_fullDuration;

    private void Start()
    {
        m_fullDuration = Duration;
    }

    // Update is called once per frame
    void Update()
    {
        Duration -= Time.deltaTime;
        if (Duration <= 0f)
        {
            if (OnDestroy != null)
            {
                OnDestroy();
            }
            if (Pool != null)
            {
                Pool.Return(gameObject);
                Duration = m_fullDuration;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
