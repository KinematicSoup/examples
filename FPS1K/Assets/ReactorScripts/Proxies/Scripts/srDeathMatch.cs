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
    
    public class srDeathMatch : ksProxyRoomScript
    {
#if UNITY_EDITOR
        public Single RoundTime;
        public Single ResetTime;
        public srDeathMatch() : base() 
        {
            RoundTime = 300f;
            ResetTime = 3f;
        }
#endif
    }
}