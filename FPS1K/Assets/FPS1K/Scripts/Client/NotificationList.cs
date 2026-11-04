using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// A list of text notifications that fadeout after a delay. When new messages are added, old ones are displaced
// upwards.
public class NotificationList : MonoBehaviour
{
    // How long in seconds before a message starts to fade.
    public float FadeDelay = 2f;
    // How long in seconds it takes a message to fade out.
    private const float FADE_TIME = .5f;

    private List<float> m_timers = new List<float>();

    public void Add(string message)
    {
        Add(message, Color.white);
    }

    public void Add(string message, Color colour)
    {
        GameObject child = transform.GetChild(0).gameObject;
        if (m_timers.Count > 0)
        {
            child = Instantiate(child, transform);
            child.transform.SetSiblingIndex(0);

            // Move other messages up
            for (int i = 1; i < transform.childCount; i++)
            {
                RectTransform ch = transform.GetChild(i) as RectTransform;
                Vector3 position = ch.localPosition;
                position.y += ch.rect.height;
                ch.localPosition = position;
            }
        }
        m_timers.Insert(0, 0f);
        TMP_Text text = child.GetComponent<TMP_Text>();
        text.text = message;
        colour.a = 1f;
        text.color = colour;
    }

    public void Clear()
    {
        while (transform.childCount > 1)
        {
            Destroy(transform.GetChild(1).gameObject);
        }
        transform.GetChild(0).GetComponent<TMP_Text>().text = "";
        m_timers.Clear();
    }

    public void Update()
    {
        for (int i = m_timers.Count - 1; i >= 0; i--)
        {
            m_timers[i] += Time.deltaTime;
            if (m_timers[i] >= FadeDelay + FADE_TIME)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (i == 0)
                {
                    child.GetComponent<TMP_Text>().text = "";
                }
                else
                {
                    Destroy(child);
                }
                m_timers.RemoveAt(i);
            }
            else if (m_timers[i] >= FadeDelay)
            {
                GameObject child = transform.GetChild(i).gameObject;
                TMP_Text text = child.GetComponent<TMP_Text>();
                Color colour = text.color;
                colour.a = 1f - (m_timers[i] - FadeDelay) / FADE_TIME;
                text.color = colour;
            }
        }
    }
}
