using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using KSProxies.Scripts;

// Draws handles in the scene for the waypoints of the selected moving platform.
[CustomEditor(typeof(sePlatform))]
class PlatformEditor : Editor
{
    private static Color32 m_color = new Color32(255, 166, 25, 64);

    void OnSceneGUI()
    {
        sePlatform platform = target as sePlatform;
        DrawWayPoints(platform, true);
        serializedObject.ApplyModifiedProperties();
    }

    public static void DrawWayPoints(sePlatform platform, bool selected)
    {
        if (!selected || platform.Waypoints == null)
        {
            return;
        }

        Handles.color = m_color;
        Vector3 wp;
        for (int i = 0; i < platform.Waypoints.Length; i++)
        {
            wp = platform.Waypoints[i].Position + platform.transform.position;

            EditorGUI.BeginChangeCheck();
            wp = Handles.DoPositionHandle(wp, platform.transform.rotation);

            if (i == 0)
            {
                if (platform.Loop)
                {
                    Handles.DrawDottedLine(platform.Waypoints[platform.Waypoints.Length - 1].Position +
                        platform.transform.position, wp, 1.0f);
                }
            }
            else if (i > 0)
            {
                Handles.DrawDottedLine(platform.Waypoints[i - 1].Position + platform.transform.position, wp, 2.0f);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(platform, "Move spawn point");
                platform.Waypoints[i].Position = wp - platform.transform.position;
            }
            Collider collider = platform.transform.GetComponent<Collider>();
            if (collider != null)
            {
                Bounds bounds = collider.bounds;
                Vector3 offset = bounds.center - platform.transform.position;
                Handles.DrawWireCube(wp + offset, bounds.extents * 2);
            }
        }
    }
}
