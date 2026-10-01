/* This file was auto-generated. DO NOT MODIFY THIS FILE. */
using System;
using UnityEngine;
using KS.Reactor.Client.Unity.Adaptors;
using KS.Reactor;

namespace KSProxies
{
    public class Fighter : ksProxyEntityScript
    {
        [ksProperty(1000)]
        public String m_playerName = "";
        [ksProperty(1002)]
        public Int32 m_teamNumber = 0;
        [ksProperty(1003)]
        public Single m_teamColorR = 0f;
        [ksProperty(1004)]
        public Single m_teamColorG = 0f;
        [ksProperty(1005)]
        public Single m_teamColorB = 0f;
        [ksProperty(1006)]
        public Single m_glowColorR = 0f;
        [ksProperty(1007)]
        public Single m_glowColorG = 0f;
        [ksProperty(1008)]
        public Single m_glowColorB = 0f;
        [ksProperty(1020)]
        public Int32 m_health = 0;
        [ksProperty(1025)]
        public Single m_energy = 0f;
        [ksProperty(1026)]
        public Boolean m_energyFrozen = false;
        [ksProperty(1030)]
        public Boolean m_usingShield = false;
        [ksProperty(1040, Smoothing = ksPropertyAttribute.Types.Smoothing.LINEAR, Min = 0f, Max = 1f)]
        public Single m_laserReloadProgress = 0f;
        [ksProperty(1041, Smoothing = ksPropertyAttribute.Types.Smoothing.LINEAR, Min = 0f, Max = 1f)]
        public Single m_missileReloadProgress = 0f;
        [ksProperty(1042)]
        public Single m_boostReloadProgress = 0f;
        [ksProperty(1043)]
        public Single m_shieldReloadProgress = 0f;
        [ksProperty(1045)]
        public UInt32 m_lockTargetID = 0;
        [ksProperty(1050)]
        public Boolean m_isTargeted = false;
        [ksProperty(1060)]
        public Boolean m_isOutOfBounds = false;
    }
}