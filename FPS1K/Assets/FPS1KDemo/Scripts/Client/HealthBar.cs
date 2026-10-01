using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A UI Health bar
public class HealthBar : MonoBehaviour
{
    private float m_maxLength = 400.0f;
    public float m_health = 1.0f;
    private float m_renderHealth = 1.0f;
    private float m_healthRate = 1.0f;
    private UnityEngine.UI.Image m_image = null;

    public Color[] Colours = new Color[] { Color.red, new Color(1f, 1f, 0f), Color.green };
    public bool LerpColours = true;
    public bool Vertical = false;

    // Use this for initialization
    void Start()
    {
        m_image = GetComponent<UnityEngine.UI.Image>();
        Vector2 size = (transform as RectTransform).sizeDelta;
        m_maxLength = Vertical ? size.y : size.x;
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (m_renderHealth > m_health)
        {
            m_renderHealth -= m_healthRate * Time.deltaTime;
            if (m_renderHealth < m_health)
            {
                m_renderHealth = m_health;
            }
        }
        else if (m_renderHealth < m_health)
        {
            m_renderHealth += m_healthRate * Time.deltaTime;
            if (m_renderHealth > m_health)
            {
                m_renderHealth = m_health;
            }
        }

        Vector2 size = (transform as RectTransform).sizeDelta;
        if (Vertical)
        {
            size.y = m_maxLength * m_renderHealth;
        }
        else
        {
            size.x = m_maxLength * m_renderHealth;
        }
        (transform as RectTransform).sizeDelta = size;

        if (m_image)
        {
            m_image.color = GetColour();
        }
    }

    public void SetHealth(float health)
    {
        m_health = health;
    }

    private Color GetColour()
    {
        if (Colours == null || Colours.Length == 0)
        {
            return Color.white;
        }
        if (m_health <= 0 || Colours.Length == 1)
        {
            return Colours[0];
        }
        if (m_health >= 1)
        {
            return Colours[Colours.Length - 1];
        }
        float step = 1f / (Colours.Length - 1);
        int index = 0;
        float value = m_health;
        while (value > step)
        {
            value -= step;
            index++;
        }
        return LerpColours ? Color.Lerp(Colours[index], Colours[index + 1], value / step) : Colours[index];
    }
}
