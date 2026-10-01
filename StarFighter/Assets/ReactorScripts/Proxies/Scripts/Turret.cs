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
    
    public class Turret : ksProxyEntityScript
    {
#if UNITY_EDITOR
        public Single m_maxEnergy;
        public Single m_energyRechargeRate;
        public Single m_energyFreezeEnd;
        public Single m_maxElevation;
        public Single m_maxDepression;
        public Single m_elevateRate;
        public Single m_rotateRate;
        public Single m_laserReloadTime;
        public Vector3 m_turretCenter;
        public Vector3 m_laserSpawnPosition;
        public Turret() : base() 
        {
            m_maxEnergy = 700f;
            m_energyRechargeRate = 45f;
            m_energyFreezeEnd = 250f;
            m_maxElevation = 75f;
            m_maxDepression = 22.5f;
            m_elevateRate = 70f;
            m_rotateRate = 120f;
            m_laserReloadTime = 0.125f;
            m_turretCenter = new Vector3(0f, 0.081f, -0.101f);
            m_laserSpawnPosition = new Vector3(0.0075f, 0f, 0.25f);
        }
#endif
    }
}