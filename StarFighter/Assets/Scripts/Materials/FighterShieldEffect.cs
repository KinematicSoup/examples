using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class FighterShieldEffect : MonoBehaviour
{
    public AudioSource AudioSource;

    public Vector2 ScrollSpeed;

    private Renderer m_renderer;
    private Vector2 m_offset;
    private float m_shieldStrength = 0;

	void Start ()
    {
        m_renderer = GetComponent<Renderer>();
        m_offset = Vector2.zero;
	}
	
	void Update ()
    {
        bool shieldActive = (GetComponent<FighterClient>() == null) ? false : GetComponent<FighterClient>().UsingShield;

        m_shieldStrength = Mathf.Lerp(m_shieldStrength, shieldActive ? 1 : 0, Time.deltaTime * 8.0f);

        m_renderer.material.SetFloat("_ShieldStrength", m_shieldStrength);
        AudioSource.volume = m_shieldStrength * 0.375f;

        if (m_shieldStrength > 0)
        {
            m_offset += ScrollSpeed * Time.deltaTime;
            m_renderer.material.SetTextureOffset("_ShieldColor", m_offset);
        }
	}
}
