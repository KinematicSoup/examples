using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes an object always face the camera. Used with sprites.
public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
