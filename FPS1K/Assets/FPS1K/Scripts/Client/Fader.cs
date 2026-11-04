using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Linearly interpolates the objects alpha value towards the target value.
public class Fader : MonoBehaviour
{
    // The current alpha value
    public float Alpha
    {
        get 
        {
            if (m_renderer != null)
            {
                return m_renderer.color.a;
            }
            if (m_line != null)
            {
                return m_line.startColor.a;
            }
            return m_material.color.a;
        }
        set
        {
            if (m_line != null)
            {
                Color color = m_line.startColor;
                color.a = value;
                m_line.startColor = color;
                color = m_line.endColor;
                color.a = Mathf.Max(0f, value + m_deltaEnd);
                m_line.endColor = color;
            }
            else
            {
                Color color = m_renderer == null ? m_material.color : m_renderer.color;
                color.a = value;
                if (m_renderer != null)
                {
                    m_renderer.color = color;
                }
                else
                {
                    m_material.color = color;
                }
            }
            enabled = m_targetAlpha != value;
        }
    }

    // The target alpha value.
    public float TargetAlpha
    {
        get { return m_targetAlpha; }
        set
        {
            m_targetAlpha = value;
            enabled = Alpha != value;
        }
    }
    [SerializeField]
    private float m_targetAlpha;

    // The number of seconds it takes to interpolate from 0 to 1.
    public float Duration = 1f;

    private LineRenderer m_line;
    private float m_deltaEnd;
    private SpriteRenderer m_renderer;
    private Material m_material;

    public float InitialAlpha
    {
        get { return m_initialAlpha; }
    }
    private float m_initialAlpha;

    private void Awake()
    {
        m_renderer = GetComponent<SpriteRenderer>();
        if (m_renderer != null)
        {
            return;
        }
        m_line = GetComponent<LineRenderer>();
        if (m_line != null)
        {
            m_deltaEnd = m_line.endColor.a - m_line.startColor.a;
            return;
        }
        Renderer renderer = GetComponent<Renderer>();
        m_material = renderer == null ? null : renderer.material;
        if (m_material == null)
        {
            Destroy(this);
            return;
        }

        m_initialAlpha = Alpha;
    }

    // Update is called once per frame
    void Update()
    {
        if (Duration <= 0f)
        {
            Alpha = m_targetAlpha;
            return;
        }
        float a = Alpha;
        if (a > m_targetAlpha)
        {
            a = Mathf.Max(a - Time.deltaTime / Duration, m_targetAlpha);
        }
        else
        {
            a = Mathf.Min(a + Time.deltaTime / Duration, m_targetAlpha);
        }
        Alpha = a;
    }
}
