using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// An atomic queue of callbacks to process on the main thread. The multithread player controllers use this to queue
// actions that need to happen on the main thread.
public class srEventQueue : ksServerRoomScript
{
    private ksAtomicQueue<Action> m_eventQueue = new ksAtomicQueue<Action>();

    // Called when the script is attached.
    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }
    
    // Called during the update cycle
    private void Update()
    {
        // Process all actions in the queue
        Action action;
        while (m_eventQueue.TryDequeue(out action))
        {
            action();
        }
    }

    public void Enqueue(Action action)
    {
        m_eventQueue.Enqueue(action);
    }
}