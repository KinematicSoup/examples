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
    
    public class spBot : ksProxyPlayerScript
    {
#if UNITY_EDITOR
        public Single MinInterval;
        public Single MaxInterval;
        public Single IdleChance;
        public Single ShootChance;
        public spBot() : base() 
        {
            MinInterval = 4f;
            MaxInterval = 16f;
            IdleChance = 0.33333334f;
            ShootChance = 0.5f;
        }
#endif
    }
}