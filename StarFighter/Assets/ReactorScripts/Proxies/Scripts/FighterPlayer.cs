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
    
    public class FighterPlayer : ksProxyPlayerScript
    {
#if UNITY_EDITOR
        public Single m_spawnHeight;
        public Single m_spawnRadius;
        public Single m_spawnAngleWidth;
        public FighterPlayer() : base() 
        {
            m_spawnHeight = 8f;
            m_spawnRadius = 105f;
            m_spawnAngleWidth = 20f;
        }
#endif
    }
}