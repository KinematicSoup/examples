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
    
    public class Fighter : ksProxyEntityScript
    {
#if UNITY_EDITOR
        public Int32 m_maxHealth;
        public Single m_maxEnergy;
        public Int32 m_missileCapacity;
        public Int32 m_startingMissiles;
        public Int32 m_shieldCapacity;
        public Int32 m_startingShields;
        public Int32 m_scoreValue;
        public Single m_energyRechargeRate;
        public Single m_energyFreezeEnd;
        public Single m_shieldDamageFraction;
        public Single m_boostEnergyDrain;
        public Single m_outOfBoundsRadius;
        public Single m_outOfBoundsSafeTime;
        public Single m_laserSpeed;
        public Single m_laserReloadTime;
        public Vector3 m_laserSpawnPosition;
        public Single m_missileLockTime;
        public Single m_missileReloadTime;
        public Single m_missileMinDot;
        public Vector3 m_missileSpawnPosition;
        public Single m_minImpulseForDamage;
        public Single m_impulseDamage;
        public Fighter() : base() 
        {
            m_maxHealth = 1200;
            m_maxEnergy = 1000f;
            m_missileCapacity = 3;
            m_startingMissiles = 1;
            m_shieldCapacity = 3;
            m_startingShields = 1;
            m_scoreValue = 18;
            m_energyRechargeRate = 50f;
            m_energyFreezeEnd = 250f;
            m_shieldDamageFraction = 0.1f;
            m_boostEnergyDrain = 150f;
            m_outOfBoundsRadius = 150f;
            m_outOfBoundsSafeTime = 10f;
            m_laserSpeed = 45f;
            m_laserReloadTime = 0.165f;
            m_laserSpawnPosition = new Vector3(-0.1095f, -0.0425f, 0f);
            m_missileLockTime = 3f;
            m_missileReloadTime = 15f;
            m_missileMinDot = 0.95f;
            m_missileSpawnPosition = new Vector3(0f, -0.041f, 0.5f);
            m_minImpulseForDamage = 1f;
            m_impulseDamage = 40f;
        }
#endif
    }
}