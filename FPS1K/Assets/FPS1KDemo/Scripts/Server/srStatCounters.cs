using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Logs stat counters at regular intervals. See StatCounter.
public class srStatCounters : ksServerRoomScript
{
    // Interval in seconds to log at.
    [ksEditable]
    public float LogInterval = 10f;

    private float m_timer;

    // Called when the script is attached.
    public override void Initialize()
    {
        m_timer = LogInterval;
        // Update at time 100 after other scripts.
        Room.OnUpdate[100] += Update;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[100] -= Update;
    }
    
    // Called during the update cycle
    private void Update()
    {
        for (int i = 0; i < StatCounter.List.Count; i++)
        {
            StatCounter.List[i].Advance();
        }
        m_timer -= Time.UnscaledDelta;
        if (m_timer > 0f)
        {
            return;
        }
        m_timer += LogInterval;
        for (int i = 0; i < StatCounter.List.Count; i++)
        {
            StatCounter.List[i].Log();
            StatCounter.List[i].Clear();
        }
    }
}