using System;
using System.Collections.Generic;
using KS.Reactor;
using KS.Reactor.Server;


/*
 * Communicates player specific infromation to and from the server, and spawns a fighter for the player when applicable.
 */
public class Player : ksServerPlayerScript
{
    [ksEditable]
    public FighterController Controller;
    // how many points are lost by dying
    [ksEditable]
    private int m_deathScorePenalty = -5;
    // how long the player waits to respawn after dying in seconds.
    [ksEditable]
    private float m_respawnTime = 5.0f;

    // if a name for a player can't be attained, use this
    private const string DEFAULT_PLAYER_NAME = "default";

    public string PlayerName
    {
        get { return Properties[ID.PROP.PLAYER.NAME]; }
        set { Properties[ID.PROP.PLAYER.NAME] = value; }
    }

    private int RespawnTimer
    {
        get { return Properties[ID.PROP.PLAYER.RESPAWN_TIME_LEFT]; }
        set { Properties[ID.PROP.PLAYER.RESPAWN_TIME_LEFT] = value; }
    }

    private PlayerType m_playerType = PlayerType.NONE;
    public PlayerType PlayerType
    {
        get { return m_playerType; }
        set
        {
            if (m_playerType == value)
            {
                return;
            }
            switch (m_playerType)
            {
                case PlayerType.FIGHTER: Scripts.Detach<FighterPlayer>(); break;
                case PlayerType.TURRET: Scripts.Detach<TurretPlayer>(); break;
            }
            m_playerType = value;
            Properties[ID.PROP.PLAYER.TYPE] = (byte)value;
            switch (m_playerType)
            {
                case PlayerType.FIGHTER: Scripts.Attach(new FighterPlayer()); break;
                case PlayerType.TURRET: Scripts.Attach(new TurretPlayer()); break;
            }
        }
    }

    private Team m_team;
    public Team Team
    {
        get { return m_team; }
        set
        {
            if (m_team == value)
            {
                return;
            }
            if (m_team != null)
            {
                m_team.RemovePlayer(Player);
            }
            m_team = value;
            Properties[ID.PROP.PLAYER.TEAM_NUMBER] = m_team == null ? -1 : m_team.Number;
            if (m_team != null)
            {
                m_team.AddPlayer(Player);
            }
        }
    }

    private int m_score;
    public int Score
    {
        get { return m_score; }
    }

    public int Kills
    {
        get { return Properties[ID.PROP.PLAYER.KILLS]; }
        set { Properties[ID.PROP.PLAYER.KILLS] = value; }
    }

    public int Deaths
    {
        get { return Properties[ID.PROP.PLAYER.DEATHS]; }
        set { Properties[ID.PROP.PLAYER.DEATHS] = value; }
    }

    public ksColor Color
    {
        get { return m_team == null ? Properties[ID.PROP.PLAYER.GLOW_COLOR].Color : m_team.Color; }
        set { Properties[ID.PROP.PLAYER.GLOW_COLOR] = value; }
    }

    private GameManager m_gameManager;
    private Timer m_respawnTimer;

    public enum SpawnStates
    {
        NEEDS_SETTINGS = 0,
        COUNTDOWN = 1,
        CAN_SPAWN = 2,
        TRY_SPAWN = 3,
        SPAWNED = 4
    }

    public SpawnStates SpawnState
    {
        get { return m_spawnState; }
    }
    private SpawnStates m_spawnState = SpawnStates.NEEDS_SETTINGS;

    /**
     * Called when the script is attached.
     */
    public override void Initialize()
	{
        Room.OnUpdate[1] += Update;
        m_gameManager = Room.Scripts.Get<GameManager>();
        m_respawnTimer = new Timer(Time, m_respawnTime);
        Properties[ID.PROP.PLAYER.TEAM_NUMBER] = -1;//-1 = no team.

    }

    /**
     * Called when the script is detached.
     */
    public override void Detached()
    {
        Room.OnUpdate[1] -= Update;
    }

    /*
     * Sets the user custom settings from the client.
     */
    public void SetPlayerSettings(string name, ksColor color)
    {
        if (m_spawnState != SpawnStates.NEEDS_SETTINGS)
        {
            return;
        }
        m_spawnState = SpawnStates.CAN_SPAWN;

        name = !string.IsNullOrWhiteSpace(name) ? GetUniquePlayerName(name) : DEFAULT_PLAYER_NAME;
        PlayerName = name;
        if (Room.Scripts.Get<GameManager>().NumberOfTeams <= 0)
        {
            Color = color;
        }
        Room.Scripts.Get<Chat>().BroadcastJoin(this);
    }

    /*
     * Removes the control of entities from a player and destroys them.
     */
    public void PlayerLeave()
    {
        if (m_playerType == PlayerType.FIGHTER)
        {
            Player.Scripts.Get<FighterPlayer>().PlayerLeave();
        }
        else if (m_playerType == PlayerType.TURRET)
        {
            Player.Scripts.Get<TurretPlayer>().PlayerLeave();
        }
    }

    /*
     * Modifies the player's kill count by some amount.
     */
    public void IncrementKills(int kills, int score)
    {
        Kills += kills;
        AddScore(score);
    }

    /*
     * Modifies the player's death count.
     */
    public void AddDeath()
    {
        Deaths++;
        AddScore(m_deathScorePenalty);
    }

    /*
     * If the game isn't over, we give the player control of an entity after a countdown time.
     */
	public void Update()
    {
        if (GameManager.GameOver)
        {
            if (m_spawnState != SpawnStates.CAN_SPAWN)
            {
                m_spawnState = SpawnStates.CAN_SPAWN;
                RespawnTimer = 0;
            }
            return;
        }

        switch (m_spawnState)
        {
            case SpawnStates.COUNTDOWN: 
                m_respawnTimer.Update();
                RespawnTimer = ksMath.CeilToInt(m_respawnTimer.Time);
                if (m_respawnTimer.IsDone)
                {
                    m_spawnState = SpawnStates.CAN_SPAWN;
                }
                break;
            case SpawnStates.TRY_SPAWN:
                Spawn();
                if (PlayerSpawned())
                {
                    m_spawnState = SpawnStates.SPAWNED;
                    Properties[ID.PROP.PLAYER.SPAWNED] = true;
                }
                break;
            case SpawnStates.SPAWNED:
                if (!PlayerSpawned())
                {
                    m_spawnState = SpawnStates.COUNTDOWN;
                    m_respawnTimer.Start();
                    RespawnTimer = ksMath.CeilToInt(m_respawnTimer.Time);
                    Properties[ID.PROP.PLAYER.SPAWNED] = false;
                }
                break;
        }
	}

    /*
     * Checks if the player has a controlled entity currently spawned.
     */
    private bool PlayerSpawned()
    {
        if (m_playerType == PlayerType.FIGHTER)
        {
            return Player.Scripts.Get<FighterPlayer>().Spawned;
        }
        else if (m_playerType == PlayerType.TURRET)
        {
            return Player.Scripts.Get<TurretPlayer>().Spawned;
        }
        return false;
    }

    public void BeginTrySpawn(PlayerType playerType)
    {
        if (m_spawnState == SpawnStates.CAN_SPAWN || m_spawnState == SpawnStates.TRY_SPAWN)
        {
            PlayerType = playerType;
            m_spawnState = SpawnStates.TRY_SPAWN;
        }
    }

    private void Spawn()
    {
        if (m_playerType == PlayerType.FIGHTER)
        {
            Player.Scripts.Get<FighterPlayer>().Spawn();
        }
        else if (m_playerType == PlayerType.TURRET)
        {
            Player.Scripts.Get<TurretPlayer>().FindTurret();
        }
    }

    /*
     * Checks if a player name is not unique, and if it is not, add a suffix to make it different.
     */
    private string GetUniquePlayerName(string name)
    {
        string finalName = name;
        int sameNameCount = 0;
        bool hasUniqueName;

        do
        {
            hasUniqueName = true;

            foreach (ksIServerPlayer serverPlayer in Room.Players)
            {
                if (serverPlayer.Id != Player.Id && serverPlayer.Scripts.Get<Player>() != null && finalName.Equals(serverPlayer.Scripts.Get<Player>().PlayerName))
                {
                    sameNameCount++;
                    hasUniqueName = false;
                    finalName = name + "(" + sameNameCount + ")";
                }
            }
        }
        while (!hasUniqueName);

        return finalName;
    }

    /*
     * Modifies the player's score and that of their team.
     */
    private void AddScore(int score)
    {
        m_score += score;
        Properties[ID.PROP.PLAYER.SCORE] = m_score;
        m_gameManager.AddScore(m_team, score);
    }
}