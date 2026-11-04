using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// A UI overlay that can display text in world-space or screen-space from 0 to 1. It must have a child with an
// OverlayText and TMP_Text component to use as the template for overlayed text.
public class TextOverlayUI : MonoBehaviour
{
    private GameObject m_template;

    private void Awake()
    {
        m_template = transform.GetChild(0).gameObject;
    }

    // Add text at a world position.
    public OverlayText Add(string message, Vector3 position)
    {
        return Add(message, position, Color.white);
    }

    // Add text with a color at a world position.
    public OverlayText Add(string message, Vector3 position, Color color)
    {
        if (m_template == null)
        {
            return null;
        }
        GameObject obj = Instantiate(m_template, transform);
        OverlayText worldText = obj.GetComponent<OverlayText>();
        TMP_Text text = obj.GetComponent<TMP_Text>();
        if (worldText != null)
        {
            worldText.Position = position;
        }
        if (text != null)
        {
            text.text = message;
            color.a = 1f;
            text.color = color;
        }
        obj.SetActive(true);
        return worldText;
    }

    // Add text at a screen position from 0 to 1.
    public OverlayText Add(string message, Vector2 position)
    {
        return Add(message, position, Color.white);
    }

    // Add text with a color at a screen position from 0 to 1.
    public OverlayText Add(string message, Vector2 position, Color color)
    {
        OverlayText text = Add(message, new Vector3(position.x, position.y, 0f), color);
        text.IsWorldPosition = false;
        return text;
    }
}
