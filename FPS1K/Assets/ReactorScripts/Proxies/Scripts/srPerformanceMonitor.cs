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
    
    public class srPerformanceMonitor : ksProxyRoomScript
    {
#if UNITY_EDITOR
        public Int32 LogInterval;
        public srPerformanceMonitor() : base() 
        {
            LogInterval = 60;
        }
#endif
    }
}