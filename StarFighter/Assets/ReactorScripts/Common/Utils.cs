using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor;

public static class Utils
{
    public static ksRandom Random
    {
        get { return m_random; }
    }
    private static ksRandom m_random = new ksRandom();

    /*
     * converts two angles in radians and a radius into a world position
     */
    public static ksVector3 SphericalToCartesian(double theta, double phi, double r)
    {
        return new ksVector3(
            (float)(r * Math.Cos(theta) * Math.Cos(phi)),
            (float)(r * Math.Sin(theta)),
            (float)(r * Math.Cos(theta) * Math.Sin(phi))
            );
    }

    public static int Quantize(float value, float precision = .01f)
    {
        return ksMath.RoundToInt(value / precision);
    }
}


/*
 * Counts down from a set time period
 */
public class Timer
{
    private float m_maxTime;
    private ksTime m_time;

    private float m_currentTime = 0;
    public float Time
    {
        get { return m_currentTime; }
    }

    public bool IsDone
    {
        get { return m_currentTime == 0; }
    }
    
    /*
     * is the timer currently active (ie. is currently counting down or just reached zero)
     */
    private bool m_isActive = false;
    public bool IsActive
    {
        get { return m_isActive; }
    }

    public Timer(ksTime time, float maxTime, bool start = false)
    {
        m_time = time;
        m_maxTime = maxTime;

        if (start)
        {
            Start();
        }
    }

    public void Update()
    {
        if (m_isActive)
        {
            if (IsDone)
            {
                m_isActive = false;
            }

            m_currentTime = Math.Max(m_currentTime - m_time.Delta, 0);
        }
    }

    /*
     * initiates the countdown
     */
    public void Start()
    {
        m_currentTime = m_maxTime;
        m_isActive = true;
    }

    /*
     * sets the time to 0 and deactivates
     */
    public void Stop()
    {
        m_currentTime = 0;
        m_isActive = false;
    }
}