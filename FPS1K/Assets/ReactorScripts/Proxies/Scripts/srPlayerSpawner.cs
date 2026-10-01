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
    
    public class srPlayerSpawner : ksProxyRoomScript
    {
#if UNITY_EDITOR
        public ksPlayerControllerAsset Controller;
        public String Prefab;
        public Int32 NumBots;
        public Boolean LoadTestBotsUnlimitedAmmo;
        public srPlayerSpawner() : base() 
        {
            Controller = null;
            Prefab = "Character";
            NumBots = 0;
            LoadTestBotsUnlimitedAmmo = true;
        }
#endif
    }
}