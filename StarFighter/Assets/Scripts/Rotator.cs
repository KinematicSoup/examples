using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float Interval = 120f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, 360f * Time.deltaTime / Interval);
    }
}
