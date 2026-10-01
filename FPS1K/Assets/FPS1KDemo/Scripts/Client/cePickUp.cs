using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Client pickup script. Makes the pickup rotate.
public class cePickUp : ksEntityScript
{
    private const float ROTATION_SPEED = 360f;

    private static Quaternion m_rotation;
    private static float m_angle;
    private static int m_lastFrame;

    public override void Initialize()
    {
        // Prevent client-side bullet raycasts from hitting pickups by disabling the collider.
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    // Called every frame.
    private void Update()
    {
        if (m_lastFrame != UnityEngine.Time.frameCount)
        {
            m_lastFrame = UnityEngine.Time.frameCount;
            m_angle += ROTATION_SPEED * Time.Delta;
            m_angle %= 360f;
            m_rotation = Quaternion.AngleAxis(m_angle, Vector3.up);
        }
        transform.GetChild(0).rotation = m_rotation;
    }
}