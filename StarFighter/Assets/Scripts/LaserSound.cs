using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class LaserSound : MonoBehaviour
{
    [Tooltip("The audio source to be used for the bullet sounds.")]
    public AudioSource BulletSound;

    [Tooltip("The audio source to be used for the bullet fire sounds.")]
    public AudioSource FireSource;

    [Tooltip("The audio clips that play when the laser is first fired.")]
    public AudioClip[] ShootSounds;

    [Tooltip("The pitch the laser fire sounds play at.")]
    [Range(0.5f, 2.0f)]
    public float BasePitch = 1.0f;

    [Tooltip("The max amount the pitch of the fire sound varies.")]
    [Range(0, 1.5f)]
    public float PitchVariation = 0.75f;

	void Start ()
    {
        FireSource.clip = ChooseShootSound();
        FireSource.pitch = BasePitch + Random.Range(0, 2 * PitchVariation) - PitchVariation;
        FireSource.Play();
	}

    private AudioClip ChooseShootSound()
    {
        int randomSound = Random.Range(0, ShootSounds.Length);
        return (AudioClip)ShootSounds.GetValue(randomSound);
    }
	
	void Update () 
    {
        if (transform.root == transform && !FireSource.isPlaying)
        {
            Destroy(gameObject);
        }
        else if (transform.root == transform)
        {
            BulletSound.Stop();
        }
	}
}
