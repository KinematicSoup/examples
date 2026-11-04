using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A configurable sound effect asset. Use with SoundManager.Instance.Play.
[CreateAssetMenu]
public class Sound : ScriptableObject
{
    public AudioClip Clip;
    public float Volume = 1f;
    public float Pitch = 1f;
    // Delay in seconds before the sound starts playing
    public float Delay;
    // The clip will start playing from this time in seconds. If longer than the length of the clip, it plays from the
    // beginning.
    public float StartTime;
    // If greater than StartTime, the clip will stop playing at this time in seconds.
    public float EndTime;
}
