using System;
using System.Collections.Generic;

/*
 * contains the information needed about a player for the scoreboard
 */
public struct ScoreboardPlayer
{
    private string m_name;
    public string Name
    {
        get { return m_name; }
    }

    private PlayerType m_type;
    public string Type
    {
        get
        {
            switch (m_type)
            {
                case PlayerType.FIGHTER: return "F";
                case PlayerType.TURRET: return "T";
            }
            return "";
        }
    }

    private int m_score;
    public int Score
    {
        get { return m_score; }
    }

    private int m_kills;
    public int Kills
    {
        get { return m_kills; }
    }

    private int m_deaths;
    public int Deaths
    {
        get { return m_deaths; }
    }

    private bool m_isAlive;
    public bool IsAlive
    {
        get { return m_isAlive; }
    }

    public ScoreboardPlayer(string name, PlayerType type, int score, int kills, int deaths, bool isAlive)
    {
        m_name = name;
        m_type = type;
        m_score = score;
        m_kills = kills;
        m_deaths = deaths;
        m_isAlive = isAlive;
    }
}
