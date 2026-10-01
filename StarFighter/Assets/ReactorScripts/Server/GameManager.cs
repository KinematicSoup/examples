using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor;
using KS.Reactor.Server;


/*
 * Responsible for teams, core game settings, and the players.
 */
public class GameManager : ksServerRoomScript
{
    // score required for a team to win
    [ksEditable]
    private int m_teamWinScore = 1000;
    // score required for a player to win when playing free-for-all.
    [ksEditable]
    private int m_playerWinScore = 400;
    // how long the game will run in minutes until the map is reset
    [ksEditable]
    private float m_gameTimeMinutes = 30.0f;
    [ksEditable]
    private float m_resetTime = 5f;
    [ksEditable]
    public int NumberOfTeams = 2;

    private readonly string[] TEAM_NAMES =
    { 
        "The Enclave",
        "The Coalition",
        "The Empire", 
        "The Drifters"
    };
    private readonly ksColor[] TEAM_COLOURS =
    {
        new ksColor(1.0f, 0.18f, 0.18f),
        new ksColor(0.18f, 0.95f, 1.0f),
        new ksColor(0.18f, 1.0f, 0.18f),
        new ksColor(0.91f, 0.18f, 1.0f)
    };

    private bool m_gameStarted;
    private static bool m_gameOver = false;
    public static bool GameOver
    {
        get { return m_gameOver; }
    }

    private List<Team> m_teams = new List<Team>();

    private float m_gameTimer;

	public override void Initialize()
    {
        Room.OnUpdate[-1] += Update;
        Room.OnUpdate[10] += PostUpdate;
        Room.OnPlayerLeave += PlayerLeave;

        Properties[ID.PROP.GAME_MANAGER.WIN_SCORE] = NumberOfTeams <= 0 ? m_playerWinScore : m_teamWinScore;
        Properties[ID.PROP.GAME_MANAGER.NUMBER_OF_TEAMS] = NumberOfTeams;

        InitializeGame();
	}

    public override void Detached()
    {
        Room.OnUpdate[-1] -= Update;
        Room.OnUpdate[10] -= PostUpdate;
        Room.OnPlayerLeave -= PlayerLeave;
    }

    private void InitializeGame()
    {
        m_gameTimer = m_gameTimeMinutes * 60f;
        Room.CallRPC(ID.RPC.GAME_TIMER, m_gameTimer);

        // populates the teams using a random name/color pair and avoids repeats
        m_teams.Clear();
        for (uint i = 0; i < NumberOfTeams; i++)
        {
            int num = -1;
            bool uniqueTeam = false;

            while (!uniqueTeam)
            {
                num = Utils.Random.Next(TEAM_NAMES.Length);
                uniqueTeam = true;

                foreach (Team usedTeam in m_teams)
                {
                    if (TEAM_NAMES[num] == usedTeam.Name)
                    {
                        uniqueTeam = false;
                    }
                }
            }

            Team team = new Team((int)i, TEAM_NAMES[num], TEAM_COLOURS[num]);
            Room.Properties[ID.PROP.TEAM.NAME + i] = team.Name;
            Room.Properties[ID.PROP.TEAM.COLOR + i] = team.Color;
            Room.Properties[ID.PROP.TEAM.SCORE + i] = team.Score;
            m_teams.Add(team);
        }
    }

    [ksRPC(ID.RPC.PLAYER_SETTINGS)]
    private void SetPlayerSettings(ksIServerPlayer player, string name, ksColor color)
    {
        Player playerScript = player.Scripts.Get<Player>();
        playerScript.SetPlayerSettings(name, color);
        playerScript.Team = PickTeam();
        // Tell the player how much time is left in the round.
        Room.CallRPC(player, ID.RPC.GAME_TIMER, m_gameTimer);
    }

    [ksRPC(ID.RPC.SPAWN)]
    private void Spawn(ksIServerPlayer player, byte playerType)
    {
        Player playerScript = player.Scripts.Get<Player>();
        playerScript.BeginTrySpawn((PlayerType)playerType);
    }

    /*
     * Removes a quitting player from their team.
     */
    private void PlayerLeave(ksIServerPlayer player)
    {
        Player leavingPlayer = player.Scripts.Get<Player>();

        if (leavingPlayer != null)
        {
            Room.Scripts.Get<Chat>().BroadcastLeave(leavingPlayer);

            leavingPlayer.Team = null;
            leavingPlayer.PlayerLeave();
        }
    }

    /*
     * Gets the team belonging to a player.
     */
    public static Team GetTeam(ksIServerPlayer player)
    {
        try
        {
            return player.Scripts.Get<Player>().Team;
        }
        catch
        {
            ksLog.Error("Unable to get Team for " + player.Id + "!");
            return null;
        }
    }

    /*
     * Finds a random free turret and returns the fighter it belongs to.
     */
    public ksIServerEntity FindTurret(ksIServerPlayer turretPlayer)
    {
        List<ksIServerEntity> freeTurrets = new List<ksIServerEntity>();

        Team team = GetTeam(turretPlayer);
        IEnumerable<ksIServerPlayer> players = team == null ? (IEnumerable<ksIServerPlayer>)Room.Players : team.Players;
        foreach (ksIServerPlayer player in players)
        {
            if (player.Scripts.Get<Player>().PlayerType == PlayerType.FIGHTER)
            {
                ksIServerEntity fighter = player.Scripts.Get<FighterPlayer>().Fighter;

                if (fighter != null && !fighter.Scripts.Get<Turret>().IsOccupied)
                {
                    freeTurrets.Add(fighter);
                }
            }
        }

        if (freeTurrets.Count > 0)
        {
            Random random = new Random();
            int index = random.Next(freeTurrets.Count);
            return freeTurrets[index];
        }

        return null;
    }

    /*
     * Modifies the score of a team and updates the score properties.
     */
    public void AddScore(Team team, int score)
    {
        if (!m_gameOver && team != null)
        {
            team.AddScore(score);
            Properties[ID.PROP.TEAM.SCORE + (uint)team.Number] = team.Score;
        }
    }

    /*
     * Resets and pauses the game if there are no connected players. Updates the game timer.
     */
    public void Update()
    {
        if (Room.ConnectedPlayerCount == 0)
        {
            if (m_gameStarted)
            {
                Time.TimeScale = 0f;
                ResetGame();
            }
            Room.SkipFrameUpdates = true;
            return;
        }
        else if (!m_gameStarted)
        {
            Time.TimeScale = 1f;
            m_gameStarted = true;
        }
        m_gameTimer -= Time.UnscaledDelta;
    }

    /*
     * End the game if the time ran out or a team/player reached the win score.
     */
    private void PostUpdate()
    {
        if (!m_gameOver)
        {
            int highestScore = int.MinValue;
            bool draw = true;
            int winScore;

            if (NumberOfTeams > 0)
            {
                winScore = m_teamWinScore;
                for (int i = 0; i < m_teams.Count; i++)
                {
                    int score = m_teams[i].Score;
                    if (score > highestScore)
                    {
                        highestScore = score;
                        draw = false;
                    }
                    else if (score == highestScore)
                    {
                        draw = true;
                    }
                }
            }
            else
            {
                winScore = m_playerWinScore;
                for (int i = 0; i < Room.Players.Count; i++)
                {
                    int score = Room.Players[i].Scripts.Get<Player>().Score;
                    if (score > highestScore)
                    {
                        highestScore = score;
                        draw = false;
                    }
                    else if (score == highestScore)
                    {
                        draw = true;
                    }
                }
            }

            if (highestScore >= winScore || m_gameTimer <= 0)
            {
                EndGame(highestScore, draw);
            }
        }
        else if (m_gameTimer <= -m_resetTime)
        {
            ResetGame();
        }
    }

    /*
     * Finds a player the smallest team that has fewest players. If there's a tie, picks a random team from the tied teams.
     */
    private Team PickTeam()
    {
        if (NumberOfTeams <= 0)
        {
            return null;
        }
        List<Team> smallestTeams = new List<Team>();
        int smallestCount = m_teams[0].Players.Count;
        for (int i = 0; i < m_teams.Count; i++)
        {
            Team team = m_teams[i];
            if (team.Players.Count < smallestCount)
            {
                smallestTeams.Clear();
                smallestTeams.Add(team);
                smallestCount = team.Players.Count;
            }
            else if (team.Players.Count == smallestCount)
            {
                smallestTeams.Add(team);
            }
        }
        return smallestTeams[Utils.Random.Next(smallestTeams.Count)];
    }

    /*
     * Notifies players about the winner. If the match was a draw winner should be -1
     * Also freezes all of the objects so nothings happens after the game ends.
     */
    private void EndGame(int winningScore, bool draw)
    {
        m_gameOver = true;
        m_gameTimer = 0f;
        Time.TimeScale = 0f;
        
        Room.CallRPC(ID.RPC.GAME_END_STATUS, winningScore, draw);
        foreach (ksIServerPlayer player in Room.Players)
        {
            player.RemoveAllControllers();
        }

        ksLog.Info("Game Over");
    }

    private void ResetGame()
    {
        m_gameOver = true;
        m_gameStarted = false;
        InitializeGame();
        Scripts.Get<InitializeMap>().Reset();
        if (NumberOfTeams > 0)
        {
            foreach (ksIServerPlayer player in Room.Players)
            {
                Player playerScript = player.Scripts.Get<Player>();
                playerScript.Team = PickTeam();
            }
        }
        m_gameOver = false;
    }
}

/*
 * Stores all the information about a team.
 */
public class Team
{
    private int m_number;
    public int Number
    {
        get { return m_number; }
    }

    private string m_name = "Computer";
    public string Name
    {
        get { return m_name; }
    }

    private int m_score = 0;
    public int Score
    {
        get { return m_score; }
    }

    private List<ksIServerPlayer> m_players = new List<ksIServerPlayer>();
    public List<ksIServerPlayer> Players
    {
        get { return m_players; }
    }

    private ksColor m_color = ksColor.Gray;
    public ksColor Color
    {
        get { return m_color; }
    }

    public Team(int number, string name, ksColor color)
    {
        m_name = name;
        m_number = number;
        m_color = color;
        m_players = new List<ksIServerPlayer>();
    }

    public void AddPlayer(ksIServerPlayer player) 
    {
        m_players.Add(player);
    }

    public void RemovePlayer(ksIServerPlayer player)
    {
        m_players.Remove(player);
    }

    public void AddScore(int score)
    {
        m_score += score;
    }
}