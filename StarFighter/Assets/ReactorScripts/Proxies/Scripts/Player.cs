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
    
    public class Player : ksProxyPlayerScript
    {
#if UNITY_EDITOR
        public ksPlayerControllerAsset Controller;
        public Int32 m_deathScorePenalty;
        public Single m_respawnTime;
        public Player() : base() 
        {
            Controller = null;
            m_deathScorePenalty = -5;
            m_respawnTime = 5f;
        }
#endif
    }
}