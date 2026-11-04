using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tints the material on the object this is attached to.
public class Tinter : MonoBehaviour
{
    public Color Color;

    // Start is called before the first frame update
    private void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = Color;
        }
    }
}
