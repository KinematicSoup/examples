using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour {

    public RectTransform GUITransform = null;
    public Text NameText = null;
    public Text RangeText = null;
    public Image Icon = null;

    private bool m_focused = false;
    private bool m_visible = false;
    private Canvas m_canvas = null;

    public Color Color
    {
        get
        {
            return Icon.color;
        }
        set
        {
            Icon.color = value;
            NameText.color = value;
            RangeText.color = value;
        }
    }

    public string TargetName
    {
        get
        {
            return NameText.text;
        }
        set
        {
            NameText.text = value;
        }
    }

    public string TargetRange
    {
        get
        {
            return RangeText.text;
        }
        set
        {
            RangeText.text = value;
        }
    }

    public bool Visible
    {
        get
        {
            return m_visible;
        }
        set
        {
            if (m_focused != value)
            {
                m_focused = value;
                GUITransform.gameObject.SetActive(m_focused);
            }
        }
    }

    public bool Focused
    {
        get
        {
            return m_focused;
        }
        set
        {
            if (m_focused != value)
            {
                m_focused = value;
                NameText.enabled = m_focused;
                RangeText.enabled = m_focused;
            }
        }
    }

    void Start ()
    {
        m_canvas = GameObject.FindGameObjectWithTag("GUI").GetComponent<Canvas>();
        GameObject playerIndicators = GameObject.FindGameObjectWithTag("PlayerIndicators");
        GUITransform.SetParent(playerIndicators.transform, false);
    }

    void Update ()
    {
        Reposition();
    }

    private void Reposition()
    {
        Vector3 relativePosition = Camera.main.transform.InverseTransformPoint(transform.position);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(Camera.main.transform.TransformPoint(relativePosition));
        Vector3 halfScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);
        screenPos -= halfScreen;
        float radarSize = halfScreen.y * 0.75f;

        if (screenPos.z > 0.0f)
        {
            if (screenPos.sqrMagnitude > radarSize * radarSize)
            {
                screenPos = screenPos.normalized * radarSize;
            }
        }
        else
        {
            Vector3 normal = screenPos;
            normal.z = 0;
            normal = normal.normalized * radarSize;
            screenPos.x = -normal.x;
            screenPos.y = -normal.y;
        }
        screenPos += halfScreen;
        GUITransform.anchoredPosition = screenPos / m_canvas.scaleFactor;
    }
}
