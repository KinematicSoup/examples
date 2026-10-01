using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Death match game mode logic.
public class srDeathMatch : ksServerRoomScript
{
    // The length of a round in seconds.
    [ksEditable]
    public float RoundTime = 300f;
    // The number of seconds after a round before a new round starts.
    [ksEditable]
    public float ResetTime = 3f;

    private float m_roundTimer;

    // Called when the script is attached.
    public override void Initialize()
    {
        m_roundTimer = RoundTime;
        Room.OnUpdate[-1] += Update;
        Room.OnPlayerJoin += PlayerJoin;
        seCharacter.OnDamage += OnDamage;
        seCharacter.OnDeath += OnDeath;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[-1] -= Update;
        Room.OnPlayerJoin -= PlayerJoin;
        seCharacter.OnDamage -= OnDamage;
        seCharacter.OnDeath -= OnDeath;
    }
    
    // Called during the update cycle
    private void Update()
    {
        if (Room.ConnectedPlayerCount == 0)
        {
            Room.SkipFrameUpdates = true;
            Time.TimeScale = 0f;
            return;
        }
        m_roundTimer -= Time.RealDelta;
        float ts = Time.TimeScale;
        Time.TimeScale = m_roundTimer > 0f ? 1f : 0f;
        if (ts != Time.TimeScale)
        {
            Room.Properties[Prop.GAME_OVER] = m_roundTimer <= 0f;
        }
        if (m_roundTimer <= -ResetTime)
        {
            // Destroy everything and create a new level.
            srLevelGenerator generator = Scripts.Get<srLevelGenerator>();
            if (generator != null)
            {
                generator.GenerateLevel();
            }
            srPlayerSpawner spawner = Scripts.Get<srPlayerSpawner>();
            foreach (ksIServerPlayer player in Room.Players)
            {
                player.Properties[Prop.SCORE] = 0;
                spawner.SpawnPlayer(player);
            }
            m_roundTimer = RoundTime;
            Room.CallRPC(RPC.ROUND_TIME, m_roundTimer);
        }
    }
    
    // Called when a player connects.
    private void PlayerJoin(ksIServerPlayer player)
    {
        Room.CallRPC(RPC.ROUND_TIME, m_roundTimer);
        player.Properties[Prop.SCORE] = 0;
    }

    private void OnDamage(seCharacter victim, ksIServerPlayer attacker, int damage)
    {
        if (attacker == null)
        {
            return;
        }
        // Add the damage done to the attacker's score, +100 for a kill.
        if (damage >= victim.Health)
        {
            attacker.Properties[Prop.SCORE] += victim.Health + 100;
        }
        else
        {
            attacker.Properties[Prop.SCORE] += damage;
        }
    }

    private void OnDeath(seCharacter character)
    {
        // Lose 100 points on player death.
        if (m_roundTimer > 0f)
        {
            character.Entity.Owner.Properties[Prop.SCORE] -= 100;
        }
    }
}