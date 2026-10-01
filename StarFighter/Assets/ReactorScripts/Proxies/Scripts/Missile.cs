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
    
    public class Missile : ksProxyEntityScript
    {
#if UNITY_EDITOR
        public Int32 EnergyCost;
        public Int32 Damage;
        public Single m_lifeTime;
        public Single m_speed;
        public Single m_maxTurnRate;
        public Missile() : base() 
        {
            EnergyCost = 400;
            Damage = 600;
            m_lifeTime = 10f;
            m_speed = 11f;
            m_maxTurnRate = 90f;
        }
#endif
    }
}