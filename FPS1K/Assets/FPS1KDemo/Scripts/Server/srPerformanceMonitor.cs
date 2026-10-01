using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Logs performance timings at regular intervals
public class srPerformanceMonitor : ksServerRoomScript
{
    // The interval in seconds to log timing data. < 0 to disable logging.
    [ksEditable]
    public int LogInterval = 60;

    // Called when the script is attached.
    public override void Initialize()
    {
        int interval = LogInterval * 1000;
        Room.UpdateTimer.LogInterval = interval;
        Room.CacheTimer.LogInterval = interval;
        Room.EncodeTimer.LogInterval = interval;
        Room.PhysicsTimer.LogInterval = interval;
        Room.ScriptsTimer.LogInterval = interval;
        Room.ControllersTimer.LogInterval = interval;
    }
}