using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class Explosion : MonoBehaviour
{
    [Tooltip("Explosion trail prefab that flies away from the explosion leaving behind smoke trails.")]
    public Transform Trails;

    [Tooltip("Number of projectiles trails.")]
    [Range(0, 20)]
    public int NumberOfTrails;

    [Tooltip("Audio clips that may be played by the explosion.")]
    public AudioClip[] Sounds;

    public AnimationCurve LightBrightness;

    [Tooltip("How bright the peaks in the Light Brighness curve are.")]
    [Range(0, 16)]
    public float LightBrightnessMultiplier = 1.0f;

    [Tooltip("How long in seconds the Light Brighness animation lasts.")]
    [Range(0, 2)]
    public float LightTotalTime = 1.0f;

    private float m_lightTime = 0;

    void Start()
    {
        GetComponent<AudioSource>().clip = (AudioClip)Sounds.GetValue((int)Random.Range(0, Sounds.Length));
        GetComponent<AudioSource>().Play();

        for (int i = NumberOfTrails; i > 0; i-- )
        {
            Transform trail = Instantiate(Trails, transform.position, Random.rotation) as Transform;
            trail.parent = transform;
        }
	}
	
	void Update () 
    {
        m_lightTime += Time.deltaTime;
        GetComponent<Light>().intensity = LightBrightnessMultiplier * LightBrightness.Evaluate(m_lightTime / LightTotalTime);

        if (!GetComponent<ParticleSystem>().IsAlive())
        {
            Destroy(gameObject);
        }
	}
}
