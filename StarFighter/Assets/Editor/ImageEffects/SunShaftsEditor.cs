using System;
using UnityEditor;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [CustomEditor (typeof(SunShafts))]
    class SunShaftsEditor : Editor
    {
        SerializedObject m_serObj;

        SerializedProperty m_sunTransform;
        SerializedProperty m_radialBlurIterations;
        SerializedProperty m_sunColor;
        SerializedProperty m_sunThreshold;
        SerializedProperty m_sunShaftBlurRadius;
        SerializedProperty m_sunShaftIntensity;
        SerializedProperty m_useDepthTexture;
        SerializedProperty m_resolution;
        SerializedProperty m_screenBlendMode;
        SerializedProperty m_maxRadius;

        void OnEnable () {
            m_serObj = new SerializedObject (target);

            m_screenBlendMode = m_serObj.FindProperty("ScreenBlendMode");

            m_sunTransform = m_serObj.FindProperty("SunTransform");
            m_sunColor = m_serObj.FindProperty("SunColor");
            m_sunThreshold = m_serObj.FindProperty("SunThreshold");

            m_sunShaftBlurRadius = m_serObj.FindProperty("SunShaftBlurRadius");
            m_radialBlurIterations = m_serObj.FindProperty("RadialBlurIterations");

            m_sunShaftIntensity = m_serObj.FindProperty("SunShaftIntensity");

            m_resolution =  m_serObj.FindProperty("Resolution");

            m_maxRadius = m_serObj.FindProperty("MaxRadius");

            m_useDepthTexture = m_serObj.FindProperty("UseDepthTexture");
        }


        public override void OnInspectorGUI () {
            m_serObj.Update ();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField (m_useDepthTexture, new GUIContent ("Rely on Z Buffer?"));
            if ((target as SunShafts).GetComponent<Camera>())
                GUILayout.Label("Current camera mode: "+ (target as SunShafts).GetComponent<Camera>().depthTextureMode, EditorStyles.miniBoldLabel);

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.PropertyField (m_resolution,  new GUIContent("Resolution"));
            EditorGUILayout.PropertyField (m_screenBlendMode, new GUIContent("Blend mode"));

            EditorGUILayout.Separator ();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField (m_sunTransform, new GUIContent("Shafts caster", "Chose a transform that acts as a root point for the produced sun shafts"));
            if ((target as SunShafts).SunTransform && (target as SunShafts).GetComponent<Camera>()) {
                if (GUILayout.Button("Center on " + (target as SunShafts).GetComponent<Camera>().name)) {
                    if (EditorUtility.DisplayDialog ("Move sun shafts source?", "The SunShafts caster named "+ (target as SunShafts).SunTransform.name +"\n will be centered along "+(target as SunShafts).GetComponent<Camera>().name+". Are you sure? ", "Please do", "Don't")) {
                        Ray ray = (target as SunShafts).GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f,0.5f,0));
                        (target as SunShafts).SunTransform.position = ray.origin + ray.direction * 500.0f;
                        (target as SunShafts).SunTransform.LookAt ((target as SunShafts).transform);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator ();

            EditorGUILayout.PropertyField (m_sunThreshold,  new GUIContent ("Threshold color"));
            EditorGUILayout.PropertyField (m_sunColor,  new GUIContent ("Shafts color"));
            m_maxRadius.floatValue = 1.0f - EditorGUILayout.Slider ("Distance falloff", 1.0f - m_maxRadius.floatValue, 0.1f, 1.0f);

            EditorGUILayout.Separator ();

            m_sunShaftBlurRadius.floatValue = EditorGUILayout.Slider ("Blur size", m_sunShaftBlurRadius.floatValue, 1.0f, 10.0f);
            m_radialBlurIterations.intValue = EditorGUILayout.IntSlider ("Blur iterations", m_radialBlurIterations.intValue, 1, 3);

            EditorGUILayout.Separator ();

            EditorGUILayout.PropertyField (m_sunShaftIntensity,  new GUIContent("Intensity"));

            m_serObj.ApplyModifiedProperties();
        }
    }
}
