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
    
    public class srSyncGroupManager : ksProxyRoomScript
    {
#if UNITY_EDITOR
        public Single ViewDistance;
        public Int32 TileLengthPerGroup;
        public srSyncGroupManager() : base() 
        {
            ViewDistance = 160f;
            TileLengthPerGroup = 2;
        }
#endif
    }
}