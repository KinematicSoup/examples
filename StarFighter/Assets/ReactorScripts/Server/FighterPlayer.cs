using System;
using System.Collections.Generic;
using KS.Reactor;
using KS.Reactor.Server;

/*
 * Controls aspects of a player specific to playing a fighter.
 */
public class FighterPlayer : ksServerPlayerScript
{
    // the elevation at which the fighters will point towards on spawn
    [ksEditable]
    private float m_spawnHeight = 8.0f;
    // how far from the center fighters spawn
    [ksEditable]
    private float m_spawnRadius = 105.0f;
    // this value is the width of a team's sector in which fighters will spawn
    [ksEditable]
    private float m_spawnAngleWidth = 20.0f;

    // the fighter this player is currently controlling
    private ksIServerEntity m_fighter;
    private static ksOverlapParams m_overlapParams;
    public ksIServerEntity Fighter
    {
        get { return m_fighter; }
    }

    public bool Spawned
    {
        get { return m_fighter != null && !m_fighter.IsDestroyed; }
    }

    /*
     * Called when the fighter player leaves the game.
     */
    public void PlayerLeave()
    {
        if (m_fighter != null && !m_fighter.IsDestroyed)
        {
            m_fighter.Scripts.Get<Turret>().TurretDestroyed(false);
            m_fighter.Destroy();
        }
    }

    /**
     * Called when the fighter is destroyed by an enemy.
     */
    public void FighterDestroyed()
    {
        m_fighter = null;
        Player.Scripts.Get<Player>().AddDeath();
    }

    /*
     * Spawns a fighter for the player on a part of the map corresponding to their team.
     * They are placed on a spherical surface section facing the center of the map on that arc.
     * If demo mode is active, fighters are spawned near the first found instance of a fighter.
     */
    public void Spawn()
    {
        ksVector3 position = GetSpawnPos();
        ksQuaternion orientation = ksQuaternion.Identity;

        // if we were unable to find a clear spawn, just wait a frame
        if (position == ksVector3.Zero)
        {
            return;
        }

        // face the center of the map
        orientation = ksQuaternion.FromDirection(new ksVector3(0, m_spawnHeight, 0) - position);

        SpawnFighter(position, orientation);
    }

    /*
     * Gets a spawn position that is not blocked and in the team's spawn area.
     */
    private ksVector3 GetSpawnPos()
    {
        float ang = m_spawnAngleWidth / 2;
        Team team = Scripts.Get<Player>().Team;

        // the azimuth angle
        float mainAngle = team == null ? Utils.Random.NextFloat(0f, 360f) :
            team.Number / (float)Room.Scripts.Get<GameManager>().NumberOfTeams * 360f + Utils.Random.NextFloat(-ang, ang);
        // the elevation angle
        float heightAngle = Utils.Random.NextFloat(-ang, ang);

        ksVector3 spawnCenter = new ksVector3(0, m_spawnHeight, 0);

        if (m_overlapParams == null)
        {
            m_overlapParams = new ksOverlapParams()
            {
                Shape = new ksSphere(.5f)
            };
        }
        // tries up to 25 times to find an empty spawn location this frame, very unlikely to fail
        for (int i = 0; i < 25; i++)
        {
            m_overlapParams.Origin = spawnCenter + Utils.SphericalToCartesian(heightAngle * (Math.PI / 180), mainAngle * (Math.PI / 180), m_spawnRadius);
            if (!Physics.OverlapAny(m_overlapParams))
            {
                return m_overlapParams.Origin;
            }
        }

        return ksVector3.Zero;
    }

    /*
     * Places a new fighter for this player.
     */
    private void SpawnFighter(ksVector3 position, ksQuaternion orientation)
    {
        FighterController controller = Player.Scripts.Get<Player>().Controller;
        ksSpawnParams spawn = new ksSpawnParams()
        {
            EntityType = ID.TYPE.FIGHTER,
            Transform = new ksTransformState(position, orientation, ksVector3.One),
            OwnerId = Player.Id,
            PlayerController = controller == null ? new FighterController() : controller.Clone()
        };
        m_fighter = Room.SpawnEntity(spawn);
    }
}