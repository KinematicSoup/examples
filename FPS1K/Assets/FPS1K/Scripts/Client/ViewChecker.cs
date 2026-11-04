using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewChecker : MonoBehaviour
{
    // Characters further from the camera than this will not be rendered.
    public float VisibleDistance = 160f;

    private static Plane[] m_frustumPlanes;
    private static float m_visibleDistance2;

    public static bool IsVisible(Renderer renderer)
    {
        return (Camera.main.transform.position - renderer.transform.position).sqrMagnitude <= m_visibleDistance2 &&
            GeometryUtility.TestPlanesAABB(m_frustumPlanes, renderer.bounds);
    }

    private void Awake()
    {
        m_visibleDistance2 = VisibleDistance * VisibleDistance;
    }


    // Update is called once per frame
    private void Update()
    {
        m_frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
    }
}
