using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class HitSound : MonoBehaviour
{
    [Tooltip("The audio clips that play when a fighter is hit.")]
    public AudioClip[] Sounds;

    [Tooltip("The pitch the hit sounds play at.")]
    [Range(0.5f, 2.0f)]
    public float BasePitch = 1.0f;

    [Tooltip("The max amount the pitch of the sound varies.")]
    [Range(0, 1.5f)]
    public float PitchVariation = 0.75f;

    void Start()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.clip = ChooseShootSound();
        source.pitch = BasePitch + Random.Range(0, 2 * PitchVariation) - PitchVariation;
        source.Play();
    }

    private AudioClip ChooseShootSound()
    {
        int randomSound = Random.Range(0, Sounds.Length);
        return (AudioClip)Sounds.GetValue(randomSound);
    }

    void Update()
    {
        if (!GetComponent<AudioSource>().isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
