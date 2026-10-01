using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Respawns a player after a delay.
public class spRespawn : ksServerPlayerScript
{
    // How long in seconds before the player respawns when they die.
    [ksEditable]
    public float RespawnDelay = 3f;

    private float m_respawnTimer;

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }
    
    // Called during the update cycle
    private void Update()
    {
        m_respawnTimer -= Time.Delta;
        if (m_respawnTimer <= 0f)
        {
            Room.OnUpdate[0] -= Update;
            Room.Scripts.Get<srPlayerSpawner>().SpawnPlayer(Player);
        }
    }

    public void StartRespawnTimer()
    {
        m_respawnTimer = RespawnDelay;
        Room.OnUpdate[0] += Update;
    }
}