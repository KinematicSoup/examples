using System.Collections;
using UnityEngine;
using UnityStandardAssets.CinematicEffects;

public class CameraEffects : MonoBehaviour
{
    private AntiAliasing m_antiAliasing;
    private Bloom m_bloom;
    private SunShafts m_sunShafts;

	void Start()
    {
        m_antiAliasing = GetComponent<AntiAliasing>();
        m_bloom = GetComponent<Bloom>();
        m_sunShafts = GetComponent<SunShafts>();
	}
	
	void Update()
    {
        if (m_antiAliasing != null)
        {
            m_antiAliasing.enabled = Settings.UseAntialiasing;
        }
        if (m_bloom != null)
        {
            m_bloom.enabled = Settings.UseBloom;
        }
        if (m_sunShafts != null)
        {
            m_sunShafts.enabled = Settings.UseBloom;
        }
	}
}
