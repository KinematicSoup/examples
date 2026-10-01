using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class ExplosionTrails : MonoBehaviour
{
    [Tooltip("Minimum speed the trail may move at.")]
    [Range(0, 20)]
    public float TrailMinSpeed = 3.0f;

    [Tooltip("Maximum speed the trail may move at.")]
    [Range(0, 20)]
    public float TrailMaxSpeed = 9.0f;

    private float m_trailSpeed;

    void Start()
    {
        m_trailSpeed = Random.Range(TrailMinSpeed, TrailMaxSpeed);
    }

	void Update () 
    {
        transform.Translate(transform.forward * m_trailSpeed * Time.deltaTime);
	}
}
