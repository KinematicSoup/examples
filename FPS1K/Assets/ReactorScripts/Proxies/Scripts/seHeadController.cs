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
        public Single PitchDownWeight;
        public Single PitchUpWeight;
        public Single NeutralPitch;
        public Single PitchSideWeight;
        public AnimationCurve YawSideWeightByPitch;
        public seHeadController() : base() 
        {
            Pivot = new Vector3(0f, 0.98f, 0f);
            Distance = 0.62f;
            PitchDownWeight = 0.75f;
            PitchUpWeight = 0.66f;
            NeutralPitch = 5f;
            PitchSideWeight = 0.06f;
            YawSideWeightByPitch = null;
        }
#endif
    }
}