using System;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;
using KS.Reactor.Client.Unity;

public class PlayerClient : ksPlayerScript
{
    public static PlayerType LocalPlayerType
    {
        get { return m_localPlayerType; }
        set { m_localPlayerType = value; }
    }
    private static PlayerType m_localPlayerType;

    public string PlayerName
    {
        get { return Properties[ID.PROP.PLAYER.NAME]; }
    }

    public int TeamNumber
    {
        get { return Properties[ID.PROP.PLAYER.TEAM_NUMBER]; }
    }

    public string TeamName
    {
        get
        {
            if (TeamNumber >= 0)
            {
                return GameManager.GetTeamName(TeamNumber);
            }
            return "";
        }
    }

    public ksColor Color
    {
        get 
        {
            if (TeamNumber >= 0)
            {
                return GameManager.GetTeamColor(TeamNumber);
            }
            return Properties[ID.PROP.PLAYER.GLOW_COLOR];
        }
    }

    public int RespawnTimeLeft
    {
        get { return Properties[ID.PROP.PLAYER.RESPAWN_TIME_LEFT]; }
    }

    public ksEntity PlayerEntity
    {
        get { return m_playerEntity; }
        set { m_playerEntity = value; }
    }
    private ksEntity m_playerEntity;

    public bool IsAlive
    {
        get { return m_playerEntity != null && !m_playerEntity.IsDestroyed; }
    }

    private GameManagerClient GameManager
    {
        get
        {
            if (m_gameManager == null)
            {
                m_gameManager = Room.GameObject.GetComponent<GameManagerClient>();
            }
            return m_gameManager;
        }
    }
    private GameManagerClient m_gameManager;
}