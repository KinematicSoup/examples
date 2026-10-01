/* This file was auto-generated. DO NOT MODIFY THIS FILE. */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using KS.Reactor.Client.Unity;
using KS.Reactor;
using KS.Unity;

namespace KSProxies.Scripts
{
    
    public class GameManager : ksProxyRoomScript
    {
#if UNITY_EDITOR
        public Int32 m_teamWinScore;
        public Int32 m_playerWinScore;
        public Single m_gameTimeMinutes;
        public Single m_resetTime;
        public Int32 NumberOfTeams;
        public GameManager() : base() 
        {
            m_teamWinScore = 1000;
            m_playerWinScore = 400;
            m_gameTimeMinutes = 30f;
            m_resetTime = 5f;
            NumberOfTeams = 2;
        }
#endif
    }
}