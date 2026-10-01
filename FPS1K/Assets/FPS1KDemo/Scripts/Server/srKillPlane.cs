using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Destroys entities that fall below a kill-plane.
public class srKillPlane : ksServerRoomScript
{
    [ksEditable]
    public float KillY = -10f;

    // Called when the script is attached.
    public override void Initialize()
    {
        Room.OnUpdate[-1] += Update;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[-1] -= Update;
    }
    
    // Called during the update cycle
    private void Update()
    {
        if (Time.Frame % 10 != 0)
        {
            return;
        }
        for (int i = 0; i < Room.DynamicEntities.Count; i++)
        {
            ksIServerEntity entity = Room.DynamicEntities[i];
            if (entity.Transform.Position.Y < KillY)
            {
                entity.Destroy();
            }
        }
    }
}