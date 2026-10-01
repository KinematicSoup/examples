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
    
    public class InitializeMap : ksProxyRoomScript
    {
#if UNITY_EDITOR
        public String[] m_asteroidPrefabs;
        public Single[] m_asteroidScales;
        public Int32 m_numAsteroids;
        public Single m_asteroidPlacementMinorRadius;
        public Single m_asteroidPlacementMajorRadius;
        public Int32 m_asteroidClumps;
        public Single m_asteroidClumpChance;
        public Single m_asteroidClumpRadius;
        public Single m_minAsteroidVelocity;
        public Single m_maxAsteroidVelocity;
        public Single m_minAsteroidAngularVelocity;
        public Single m_maxAsteroidAngularVelocity;
        public Single m_asteroidBaseMass;
        public Single m_largeAsteroidChance;
        public String[] m_cloudPrefabs;
        public Int32 m_cloudAmount;
        public Single m_cloudPlacementMinorRadius;
        public Single m_cloudPlacementMajorRadius;
        public InitializeMap() : base() 
        {
            m_asteroidPrefabs = new String[] {"asteroid1_1", "asteroid2_1", "asteroid3_1", "asteroid4_1", "asteroid5_1", "asteroid6_1", "asteroid7_1", "asteroid8_1"};
            m_asteroidScales = new Single[] {0.6f, 1f, 2f, 4f};
            m_numAsteroids = 400;
            m_asteroidPlacementMinorRadius = 42.5f;
            m_asteroidPlacementMajorRadius = 100f;
            m_asteroidClumps = 7;
            m_asteroidClumpChance = 0.7f;
            m_asteroidClumpRadius = 13f;
            m_minAsteroidVelocity = 0f;
            m_maxAsteroidVelocity = 0.1f;
            m_minAsteroidAngularVelocity = 0.5f;
            m_maxAsteroidAngularVelocity = 6f;
            m_asteroidBaseMass = 50f;
            m_largeAsteroidChance = 0.05f;
            m_cloudPrefabs = new String[] {"dustCloud1", "dustCloud2", "dustCloud3"};
            m_cloudAmount = 8;
            m_cloudPlacementMinorRadius = 50f;
            m_cloudPlacementMajorRadius = 100f;
        }
#endif
    }
}