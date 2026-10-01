using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;


/*
 * plays the various music tracks for the game, alternating between two audiosources for crossfades
 */
public class GameMusic : MonoBehaviour
{
    [Tooltip("The music tracks that may be played.")]
    public AudioClip[] Tracks;

    [Tooltip("The sound that is played when the player wins a match.")]
    public AudioClip WinTrack;

    [Tooltip("The sound that is played when the player loses a match.")]
    public AudioClip LoseTrack;

    [Tooltip("How many times a track will play before a new track is selected.")]
    [Range(1, 10)]
    public int LoopsPerTrack = 3;

    [Tooltip("The volume of the music.")]
    [Range(0, 1)]
    public float MaxVolumeMultiplier = 0.5f;

    private ServerConnection m_connection;
    private GameManagerClient m_gameManager;
    private AudioSource[] m_sources;
    private int m_loops = 0;
    private float m_lastClipTime = 0;
    private int m_mainSource = 0;
    private bool m_gameOver = false;


    /*
     * returns the audio source that is playing or is being switched from
     */
    private AudioSource Primary
    {
        get
        {
            return m_sources[m_mainSource];
        }
    }

    /*
     * returns the audio source that is not playing or is being switched to
     */
    private AudioSource Secondary
    {
        get
        {
            if (m_mainSource == 0)
            {
                return m_sources[1];
            }
            else
            {
                return m_sources[0];
            }
        }
    }

    /*
     * how loud a soundtrack may be
     */
    private float MaxVolume
    {
        get { return Settings.MusicVolume * MaxVolumeMultiplier; }
    }


    /*
     * starts the game off playing a random track
     */
	void Start ()
    {
        m_connection = GameObject.FindGameObjectWithTag("GameController").GetComponent<ServerConnection>();

        m_sources = transform.GetComponents<AudioSource>();

        PlayPrimary();
	}

    /*
     * Plays one track until it loops a certain number of times, after which it crossfades into a new track until game ends, after which the main tracks fade out
     */
	void Update ()
    {
        if (m_gameManager == null && m_connection.IsConnected())
        {
            m_gameManager = m_connection.Room.GameObject.GetComponent<GameManagerClient>();
        }

        if (!m_gameOver)
        {
            if (m_gameManager != null && m_gameManager.GameOver)
            {
                m_gameOver = true;
                m_sources[2].clip = m_gameManager.MatchResult == GameManagerClient.GameResult.WIN ? WinTrack : LoseTrack;
                m_sources[2].volume = MaxVolumeMultiplier;
                m_sources[2].Play();
            }
        }
        else if (!m_gameManager.GameOver)
        {
            m_gameOver = false;
            m_sources[2].Stop();
            PlayPrimary();
        }

        if (!m_gameOver)
        {
            // counts loops by observing when the active audio source moves earlier in the track compared to last frame
            if (Primary.time < m_lastClipTime)
            {
                m_loops++;
            }

            if (m_loops == LoopsPerTrack && !Secondary.isPlaying)
            {
                Secondary.clip = PickNewTrack(Primary.clip);
                Secondary.Play();
            }
            else if (m_loops == LoopsPerTrack)
            {
                Primary.volume = Mathf.Lerp(Primary.volume, 0, Time.deltaTime * 2.0f);
                Secondary.volume = Mathf.Lerp(Secondary.volume, MaxVolume, Time.deltaTime * 2.0f);

                if (Primary.volume <= 0.01f)
                {
                    Primary.Stop();
                    m_mainSource = (m_mainSource == 0 ? 1 : 0);
                    m_loops = 0;
                }
            }

            m_lastClipTime = Primary.time;
        }
        else
        {
            Primary.volume = Mathf.Lerp(Primary.volume, 0, Time.deltaTime * 2.0f);
            Secondary.volume = Mathf.Lerp(Secondary.volume, 0, Time.deltaTime * 4.0f);
        }
	}

    private void PlayPrimary()
    {
        Primary.volume = MaxVolume;

        Primary.clip = RandomTrack();
        Primary.Play();
        Secondary.Stop();
    }

    /*
     * Selects a random music track
     */
    private AudioClip RandomTrack()
    {
        return Tracks[(int)Random.Range(0, Tracks.Length)];
    }

    /*
     * Selects a random music track different from the one passed in if possible
     */
    private AudioClip PickNewTrack(AudioClip currentTrack)
    {
        if (Tracks.Length > 1)
        {
            AudioClip newTrack = RandomTrack();

            while (newTrack == currentTrack) 
            {
                newTrack = RandomTrack();
            }

            return newTrack;
        }
        else
        {
            return RandomTrack();
        }
    }
}
