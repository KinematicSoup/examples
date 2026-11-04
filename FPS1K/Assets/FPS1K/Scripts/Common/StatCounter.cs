using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KS.Reactor;

// Counter for tracking min, max, and average occurences per frame of a named stat. Eg. call Add before every physics
// query to track the number of queries made. To use you must attach the srStatCounters script to your room.
public class StatCounter
{
    public int Count
    {
        get { return m_count; }
    }

    private int m_count;
    private int m_min = int.MaxValue;
    private int m_max;
    private int m_total;
    private int m_samples;
    private string m_name;

    public static List<StatCounter> List = new List<StatCounter>();

    public StatCounter(string name)
    {
        m_name = name;
        List.Add(this);
    }

    public void Add()
    {
        lock (this)
        {
            m_count++;
        }
    }

    // Called once per frame to record the count and start a new count
    public void Advance()
    {
        m_min = Math.Min(m_count, m_min);
        m_max = Math.Max(m_count, m_max);
        m_total += m_count;
        m_samples++;
        m_count = 0;
    }

    // Logs the min, max, and average.
    public void Log()
    {
        ksLog.Info(m_name, "min=" + m_min + ", max=" + m_max + ", avg=" + (m_total / (float)m_samples));
    }

    // Resets all counters.
    public void Clear()
    {
        m_min = int.MaxValue;
        m_max = 0;
        m_total = 0;
        m_samples = 0;
    }
}
