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
    
    public class Laser : ksProxyEntityScript
    {
#if UNITY_EDITOR
        public Int32 EnergyCost;
        public Int32 Damage;
        public Single m_lifeTime;
        public Laser() : base() 
        {
            EnergyCost = 15;
            Damage = 75;
            m_lifeTime = 2f;
        }
#endif
    }
}