using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KS.Reactor.Dev.Editor
{
    [InitializeOnLoad]
    class AutoSnap
    {
        private static readonly string CONFIG_KEY = "__ksAutoSnap";

        private static bool m_enabled = false;
        private static GameObject m_lastObject;
        private static Vector3 m_lastPosition;

        static AutoSnap()
        {
            m_enabled = EditorPrefs.GetBool(CONFIG_KEY, false);
            if (m_enabled)
            {
                EditorApplication.update += Update;
            }
        }

        [MenuItem("Auto Snap/Toggle _F1")]
        private static void Toggle()
        {
            if (m_enabled)
            {
                m_enabled = false;
                EditorApplication.update -= Update;
            }
            else
            {
                m_enabled = true;
                EditorApplication.update += Update;
            }
            EditorPrefs.SetBool(CONFIG_KEY, m_enabled);
        }

        private static void Update()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            if (gameObjects.Length <= 0)
            {
                m_lastObject = null;
                return;
            }
            if (gameObjects[0] == m_lastObject && gameObjects[0].transform.position != m_lastPosition)
            {
                Snap(gameObjects);
            }
            m_lastObject = gameObjects[0];
            m_lastPosition = m_lastObject.transform.position;
        }

        private static void Snap(GameObject[] gameObjects)
        {
            float snapX = EditorPrefs.GetFloat("MoveSnapX", .25f);
            float snapY = EditorPrefs.GetFloat("MoveSnapY", .25f);
            float snapZ = EditorPrefs.GetFloat("MoveSnapZ", .25f);
            //float rotationSnap = EditorPrefs.GetFloat("RotationSnap", 15f);
            //float scaleSnap = EditorPrefs.GetFloat("ScaleSnap", 0.1f);
            HashSet<GameObject> selection = new HashSet<GameObject>(gameObjects);
            foreach (GameObject gameObject in gameObjects)
            {
                if (IsAncestorSelected(gameObject, selection))
                {
                    continue;
                }
                Vector3 p = gameObject.transform.position;
                p.x = Round(p.x, snapX);
                p.y = Round(p.y, snapY);
                p.z = Round(p.z, snapZ);
                gameObject.transform.position = p;
            }
        }

        private static bool IsAncestorSelected(GameObject gameObject, HashSet<GameObject> selection)
        {
            while (gameObject.transform.parent != null)
            {
                gameObject = gameObject.transform.parent.gameObject;
                if (selection.Contains(gameObject))
                {
                    return true;
                }
            }
            return false;
        }

        private static float Round(float value, float snapValue)
        {
            return snapValue * Mathf.Round(value / snapValue);
        }
    }
}
