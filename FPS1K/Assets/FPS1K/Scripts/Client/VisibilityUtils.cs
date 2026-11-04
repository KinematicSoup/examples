using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VisibilityUtils
{
    // Checks if a point is visble or within the nearDistance, but no further than the maxDistance.
    public static bool IsNearOrVisible(Vector3 point, float nearDistance = 5f, float maxDistance = 100f)
    {
        float dist2 = (Camera.main.transform.position - point).sqrMagnitude;
        return dist2 <= nearDistance * nearDistance || (dist2 <= maxDistance * maxDistance && IsVisible(point));
    }

    // Checks if a point is within the view frustum. Does not check for blocking geometry.
    public static bool IsVisible(Vector3 point)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(point);
        return viewportPoint.z >= 0 && viewportPoint.x >= 0 && viewportPoint.y >= 0 && viewportPoint.x <= 1 && 
            viewportPoint.y <= 1;
    }
}
