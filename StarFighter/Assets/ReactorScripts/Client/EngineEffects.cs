using System;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor.Client.Unity;

/*
 * Changes the fighter's engine effects based on the speed
 */
public class EngineEffects : ksEntityScript
{
    [Tooltip("The AudioSource that plays the engine sound.")]
    public AudioSource AudioSource;

    [Tooltip("How loud the engine is at the fighter's slowest speed.")]
    [Range(0, 1)]
    public float MinVolume = 0.1f;

    [Tooltip("How loud the engine is at the fighter's fastest speed. Can go higher when boosting.")]
    [Range(0, 1)]
    public float MaxVolume = 0.4f;

    [Tooltip("Engine pitch at the fighter's slowest speed.")]
    [Range(0, 2)]
    public float MinPitch = .8f;

    [Tooltip("Engine pitch at the fighter's fastest speed. Can go higher when boosting.")]
    [Range(0, 2)]
    public float MaxPitch = .95f;

    [Tooltip("How many particles are emmited per second at the fighter's slowest speed.")]
    [Range(5, 20)]
    public int MinExaustRate = 20;

    [Tooltip("How many particles are emmited per second at the fighter's fastest speed. Can go higher when boosting.")]
    [Range(15, 40)]
    public int MaxExaustRate = 40;

    private ParticleSystem.EmissionModule m_engineEmission;
    private AudioSource m_audioSource;
    private float m_engineStrength;

    public override void Initialize()
    {
        m_engineEmission = transform.Find("engineExhaust").GetComponent<ParticleSystem>().emission;
        m_audioSource = GetComponent<AudioSource>();
    }

    private void Update()
	{
        ksInputManager input = ksReactor.InputManager;
        float targetEngineStrength = Mathf.Min(1f,
            Mathf.Abs(input.GetAxis(ID.CONTROLS.ACCELERATE)) + Mathf.Abs(input.GetAxis(ID.CONTROLS.STRAFE)));
        if (input.GetButton(ID.CONTROLS.BOOST) && Properties[ID.PROP.FIGHTER.BOOST_RELOAD] == 0)
        {
            targetEngineStrength *= 2f;
        }
        float t = .05f * Time.Delta * 60f;
        m_engineStrength = Mathf.Lerp(m_engineStrength, targetEngineStrength, t);

        m_audioSource.volume = Mathf.LerpUnclamped(MinVolume, MaxVolume, m_engineStrength);
        m_audioSource.pitch = Mathf.LerpUnclamped(MinPitch, MaxPitch, m_engineStrength);
        m_engineEmission.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.LerpUnclamped(MinExaustRate, MaxExaustRate, m_engineStrength));
	}
}