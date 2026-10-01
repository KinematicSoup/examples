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
    
    public class seExplosiveBullet : KSProxies.Scripts.seBullet
    {
#if UNITY_EDITOR
        public Single ExplosionRadius;
        public seExplosiveBullet() : base() 
        {
            ExplosionRadius = 1f;
            LifeTime = 10f;
            Radius = 0.05f;
        }
#endif
    }
}