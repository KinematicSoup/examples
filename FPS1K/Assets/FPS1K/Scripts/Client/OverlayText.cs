using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;
using TMPro;

// UI text that is overlayed over the game. Can be in world space or screen space. This is used to show damage numbers.
public class OverlayText : MonoBehaviour
{
    // The position of the text. If IsWorldSpace is true, this is in world space. Otherwise the X, Y values are from
    // 0 to 1 in screen space and Z is ignored.
    public Vector3 Position
    {
        get { return m_position; }
        set { m_position = value; }
    }
    private Vector3 m_position;

    // If true, the text will not fade away or move vertically.
    public bool IsFixed
    {
        get { return m_isFixed; }
        set { m_isFixed = value; }
    }
    private bool m_isFixed = false;

    // Is the text positioned in world space or in screen space from 0 to 1?
    public bool IsWorldPosition
    {
        get { return m_isWorldPosition; }
        set { m_isWorldPosition = value; }
    }
    private bool m_isWorldPosition = true;

    // How long in seconds before the text starts to fade away.
    public float FadeDelay = .5f;
    // How long it takes the text to fade out.
    public float FadeDuration = .5f;
    // The text's y speed, in pixels per second.
    public float SpeedY = 0f;

    private float m_offsetY = 0f;

    private float m_timer;
    private TMP_Text m_text;

    // Start is called before the first frame update
    private void Start()
    {
        m_timer = FadeDelay + FadeDuration;
        m_text = GetComponent<TMP_Text>();
    }

    private void LateUpdate()
    {
        Vector3 point = m_isWorldPosition ?
            Camera.main.WorldToScreenPoint(m_position) :
            new Vector3(m_position.x * Screen.width, m_position.y * Screen.height, 0f);
        if (point.z < 0)
        {
            // It's behind the camera. Hide the text.
            if (m_text != null)
            {
                m_text.enabled = false;
            }
            return;
        }
        if (m_text != null)
        {
            m_text.enabled = true;
        }
        ((RectTransform)transform).anchoredPosition = new Vector2(point.x, point.y + m_offsetY);
        if (m_isFixed)
        {
            return;
        }
        m_offsetY += SpeedY * Time.deltaTime;
        m_timer -= Time.deltaTime;
        if (m_timer >= FadeDuration)
        {
            return;
        }
        if (m_timer <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        if (m_text == null)
        {
            return;
        }
        float a = m_timer / FadeDuration;
        Color color = m_text.color;
        color.a = a;
        m_text.color = color;
    }
}
