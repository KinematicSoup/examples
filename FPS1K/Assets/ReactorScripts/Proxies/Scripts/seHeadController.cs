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
    
    public class seHeadController : ksProxyEntityScript
    {
#if UNITY_EDITOR
        public Vector3 Pivot;
        public Single Distance;
        public Single PitchWeight;
        public Single YawWeight;
        public seHeadController() : base() 
        {
            Pivot = new Vector3(0f, 0.98f, 0f);
            Distance = 0.62f;
            PitchWeight = 0.66f;
            YawWeight = 0.33f;
        }
#endif
    }
}