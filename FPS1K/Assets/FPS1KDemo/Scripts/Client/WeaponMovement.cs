using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;

// Controls weapon movement and footsteps for the local player.
public class WeaponMovement : MonoBehaviour
{
    // Distance to move the weapon up and down.
    public float DistanceY = .01f;
    // Distance to move the weapon back when firing.
    public float RecoilDistance = .01f;
    // How long in seconds it takes to move the weapon up and down when PlaybackSpeed is 1.
    public float Interval = 1f / 3f;
    // How long in seconds it takes to move the weapon from recoil position back to the normal position.
    public float RecoilDuration = .15f;
    public Sound FootStep;

    // Controls walking speed. Set to 0 to stop walking.
    public float PlaybackSpeed
    {
        get { return m_playbackSpeed; }
        set { m_playbackSpeed = value; }
    }
    private float m_playbackSpeed = 0f;

    private float m_t;
    private float m_recoil;
    private Vector3 m_origin;

    // Start is called before the first frame update
    private void Start()
    {
        m_origin = transform.localPosition;
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 position = transform.localPosition;
        if (m_playbackSpeed != 0f)
        {
            float oldT = m_t;
            m_t += Time.deltaTime * m_playbackSpeed;
            // The weapon reaches the bottom at t=.75, so that's when we play the footstep sound.
            if (oldT < .75f * Interval && m_t >= .75 * Interval)
            {
                SoundManager.Instance.Play(FootStep);
            }
            m_t %= Interval;
            position.y = m_origin.y + Mathf.Sin(m_t * ksMath.FTWO_PI / Interval) * DistanceY;
        }
        if (m_recoil > 0f)
        {
            m_recoil = Mathf.Max(0f, m_recoil - RecoilDistance * Time.deltaTime / RecoilDuration);
            position.z = m_origin.z - m_recoil;
        }
        transform.localPosition = position;
    }

    public void Recoil()
    {
        m_recoil = RecoilDistance;
    }
}
