using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class DiscreteSlider : MonoBehaviour
{
    public int StepSize =  5;
    public int MinValue = 0;
    public int MaxValue = 100;
    public int InitialValue = 100;
    public Text ValueText;

    public int RawValue
    {
        get { return (int)m_slider.value; }
        set 
        {
            if (m_slider.value != value)
            {
                m_slider.value = value;
                HandleChange(m_slider.value);
            }
        }
    }

    public float Value
    {
        get { return (m_slider.value * StepSize + MinValue) / 100f; }
        set { RawValue = Mathf.RoundToInt((value * 100f - MinValue) / StepSize); }
    }

    private Slider m_slider;

    public event Action<DiscreteSlider> OnChange;

    private void Awake()
    {
        m_slider = GetComponent<Slider>();
        m_slider.wholeNumbers = true;
        m_slider.minValue = 0;
        m_slider.maxValue = (MaxValue - MinValue) / StepSize;
        m_slider.value = (InitialValue - MinValue) / StepSize;
        m_slider.onValueChanged.AddListener(HandleChange);
        UpdateText();
    }

    private void HandleChange(float value)
    {
        UpdateText();
        if (OnChange != null)
        {
            OnChange(this);
        }
    }

    private void UpdateText()
    {
        if (ValueText != null)
        {
            ValueText.text = (RawValue * StepSize + MinValue) + "%";
        }
    }
}
