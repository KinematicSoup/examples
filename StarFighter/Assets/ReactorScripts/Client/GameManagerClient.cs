using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KS.Reactor;
using KS.Reactor.Client.Unity;

/*
 * recieves and stores information about the game, such as teams and players
 */
public class GameManagerClient : ksRoomScript
{
    public int NumberOfTeams
    {
        get { return Properties[ID.PROP.GAME_MANAGER.NUMBER_OF_TEAMS]; }
    }

    public int GameWinScore
    {
        get { return Properties[ID.PROP.GAME_MANAGER.WIN_SCORE]; }
    }

    private float m_gameTimer = float.MinValue;
    public float GameTimer
    {
        get { return m_gameTimer; }
    }

    private bool m_gameOver = false;
    public bool GameOver
    {
        get { return m_gameOver; }
    }

    public enum GameResult { WIN, LOSS, DRAW, NONE };
    private GameResult m_matchResult = GameResult.NONE;
    public GameResult MatchResult
    {
        get { return m_matchResult; }
    }

    /*
     * Sets the game timer.
     */
    [ksRPC(ID.RPC.GAME_TIMER)]
    private void SetGameTimer(float gameTimer)
    {
        m_gameOver = false;
        // Add the time for server frames we already processed this frame.
        m_gameTimer = gameTimer + Time.ProcessedServerUnscaledDelta;
    }

    /*
     * receives information about the end state of the game
     */
    [ksRPC(ID.RPC.GAME_END_STATUS)]
    private void EndGame(int winningScore, bool draw)
    {
        m_gameOver = true;
        m_gameTimer = 0f;

        int playerScore = Room.LocalPlayer.Properties[ID.PROP.PLAYER.SCORE];

        if (winningScore == playerScore && draw)
        {
            m_matchResult = GameResult.DRAW;
        }
        else if (winningScore == playerScore)
        {
            m_matchResult = GameResult.WIN;
        }
        else
        {
            m_matchResult = GameResult.LOSS;
        }
    }

    public void Update()
    {
        if (!m_gameOver)
        {
            m_gameTimer -= Time.ProcessedServerUnscaledDelta;
        }
    }

    /*
     * gets the time left as a string formated mm:ss
     */
    public string GameTimeLeft()
    {
        float timeLeft = Mathf.Max(m_gameTimer / 60f, 0);

        string minutes = Mathf.Floor(timeLeft).ToString();
        string seconds = ((int)(60 * (timeLeft - Mathf.Floor(timeLeft)))).ToString();

        return minutes + ":" + (seconds.Length == 1 ? "0" + seconds : seconds);
    }

    public string GetTeamName(int teamNumber)
    {
        if (teamNumber < 0 || teamNumber >= NumberOfTeams)
        {
            return "";
        }
        return Properties[ID.PROP.TEAM.NAME + (uint)teamNumber];
    }

    public Color GetTeamColor(int teamNumber)
    {
        if (teamNumber < 0 || teamNumber >= NumberOfTeams)
        {
            return Color.black;
        }
        return Properties[ID.PROP.TEAM.COLOR + (uint)teamNumber];
    }

    public int GetTeamScore(int teamNumber)
    {
        if (teamNumber < 0 || teamNumber >= NumberOfTeams)
        {
            return 0;
        }
        return Properties[ID.PROP.TEAM.SCORE + (uint)teamNumber];
    }

    /*
     * gets the ranking of a team
     */
    public string GetTeamRank(int teamNumber)
    {
        if (teamNumber < 0)
        {
            return "n/a";
        }
        int rank = 1;
        int score = GetTeamScore(teamNumber);
        for (int i = 0; i < NumberOfTeams; i++)
        {
            if (i != teamNumber && GetTeamScore(i) > score)
            {
                rank++;
            }
        }

        switch (rank)
        {
            case 1: return "1st";
            case 2: return "2nd";
            case 3: return "3rd";
            case 4: return "4th";
            default: return "";
        }
    }

    /**
     * returns a list of ScoreboardPlayers sorted by score
     */
    public List<ScoreboardPlayer> GetPlayers(int teamNumber)
    {
        List<ScoreboardPlayer> players = new List<ScoreboardPlayer>();
        for (int i = 0; i < Room.Players.Count; i++)
        {
            ksPlayer player = Room.Players[i];
            if (teamNumber < 0 || player.Properties[ID.PROP.PLAYER.TEAM_NUMBER].Int == teamNumber)
            {
                players.Add(new ScoreboardPlayer(
                    player.Properties[ID.PROP.PLAYER.NAME],
                    (PlayerType)player.Properties[ID.PROP.PLAYER.TYPE].AsByte(),
                    player.Properties[ID.PROP.PLAYER.SCORE],
                    player.Properties[ID.PROP.PLAYER.KILLS],
                    player.Properties[ID.PROP.PLAYER.DEATHS],
                    player.Properties[ID.PROP.PLAYER.SPAWNED]));
            }
        }
        return players.OrderByDescending(p => p.Score).ToList();
    }
}