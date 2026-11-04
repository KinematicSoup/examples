using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Shows the frame rate in the UI.
public class crMetrics : ksRoomScript
{
    public float Interval = 1f;
    private double m_lastTime;
    private int m_frames;

    // Called after properties are initialized.
    public override void Initialize()
    {
        m_lastTime = UnityEngine.Time.time;
    }

    // Called every frame.
    private void Update()
    {
        m_frames++;
        double dt = UnityEngine.Time.time - m_lastTime;
        if (dt < Interval)
        {
            return;
        }
        m_lastTime = UnityEngine.Time.time;
        Hud.Instance.FrameRate.text = (m_frames / dt).ToString("F2") + " fps";
        Hud.Instance.Bandwidth.text = (Room.NetStats.Get(ksNetStats.CounterType.RX_TOTAL, true) / (1000 * dt)).ToString("F2") + " kBps";
        m_frames = 0;
    }
}