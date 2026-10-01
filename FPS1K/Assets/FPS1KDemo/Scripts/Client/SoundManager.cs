using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;

// Manages active sound effects. It must have a child game object with an AudioSource that will be copied when a new
// AudioSource is needed to play a sound, and returned to the pool of available AudioSources when the sound is done
// playing.
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance
    {
        get { return m_instance; }
    }
    private static SoundManager m_instance;

    private struct SoundInstance
    {
        public AudioSource Source;
        public float Volume;
        public bool Pooled;

        public SoundInstance(AudioSource source, float volume, bool pooled)
        {
            Source = source;
            Volume = volume;
            Pooled = pooled;
        }
    }

    private float Volume
    {
        get { return m_volume; }
        set
        {
            if (m_volume != value)
            {
                m_volume = value;
                foreach (SoundInstance sound in m_activeSounds)
                {
                    if (sound.Source != null)
                    {
                        sound.Source.volume = sound.Volume * value;
                    }
                }
            }
        }
    }
    private float m_volume = 1f;

    private GameObject m_template;
    private ksLinkedList<SoundInstance> m_activeSounds = new ksLinkedList<SoundInstance>();


    // Start is called before the first frame update
    void Start()
    {
        m_template = transform.childCount == 0 ? null : transform.GetChild(0).gameObject;
        if (m_template == null || m_template.GetComponent<AudioSource>() == null)
        {
            ksLog.Warning(this, "Invalid setup. Sound manager must have a child object with an AudioSource.");
            return;
        }
        m_template.SetActive(false);

        m_instance = this;
        Volume = Config.Instance.Volume;
        Config.Instance.OnVolumeChange += VolumeChange;

    }

    private void OnDestroy()
    {
        Config.Instance.OnVolumeChange -= VolumeChange;
    }

    private void VolumeChange(float value)
    {
        Volume = value;
    }

    private void Update()
    {
        foreach (SoundInstance sound in m_activeSounds)
        {
            if (sound.Source == null || !sound.Source.isPlaying)
            {
                m_activeSounds.RemoveCurrent();
                if (sound.Pooled && sound.Source != null)
                {
                    sound.Source.gameObject.SetActive(false);
                    // Return the source to the pool of children on this object.
                    sound.Source.transform.SetParent(transform);
                    sound.Source.transform.localPosition = Vector3.zero;
                }
            }
        }
    }

    public AudioSource Play(Vector3 position, Sound sound, bool loop = false)
    {
        AudioSource source = Play(sound, null, loop);
        source.transform.position = position;
        return source;
    }

    public AudioSource Play(Sound sound, GameObject sourceObj = null, bool loop = false)
    {
        if (sound == null)
        {
            return null;
        }
        AudioSource source = sourceObj == null ? null : sourceObj.GetComponent<AudioSource>();
        bool pooled = source == null;
        if (pooled)
        {
            // The source object did not have an audio source. Get one from the pool.
            source = GetSource();
            // If there is no source object, use the main camera as the source.
            if (sourceObj == null)
            {
                sourceObj = Camera.main.gameObject;
            }
            // Attach the audio source to the souce object.
            if (sourceObj != null)
            {
                if (loop)
                {
                    source.transform.SetParent(sourceObj.transform);
                    source.transform.localPosition = Vector3.zero;
                }
                else
                {
                    source.transform.position = sourceObj.transform.position;
                }
            }
        }
        source.clip = sound.Clip;
        source.time = sound.StartTime > 0 && sound.StartTime < sound.Clip.length ? sound.StartTime : 0f;
        source.volume = sound.Volume * m_volume;
        source.pitch = sound.Pitch;
        source.loop = loop;
        source.Play();
        if (sound.Delay > 0)
        {
            source.SetScheduledStartTime(AudioSettings.dspTime + sound.Delay);
        }
        if (sound.EndTime > sound.StartTime)
        {
            source.SetScheduledEndTime(AudioSettings.dspTime + sound.Delay + sound.EndTime - sound.StartTime);
        }
        m_activeSounds.Add(new SoundInstance(source, sound.Volume, pooled));
        return source;
    }

    private AudioSource GetSource()
    {
        // If there's more than one child, return the last one. If there's only one child, it's the template.
        AudioSource source = transform.childCount > 1 ?
            transform.GetChild(transform.childCount - 1).GetComponent<AudioSource>() : null;
        if (source == null)
        {
            // Create a new source from the template.
            source = Instantiate(m_template).GetComponent<AudioSource>();
        }
        source.transform.SetParent(null);
        source.gameObject.SetActive(true);
        source.enabled = true;
        return source;
    }
}
