using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

/*
 * hides the inner or outer dome when the main camera can't view it to prevent rendering the expensive shield effect twice pointlessly
 */
public class DomeHide : MonoBehaviour 
{
    public bool IsInnerDome;

	void LateUpdate () 
    {
        Vector3 pos = Camera.main.transform.position;

        if (IsInnerDome && (pos.y < -1 || pos.magnitude > 31))
        {
            GetComponent<Renderer>().enabled = false;
        }
        else if (!IsInnerDome && pos.magnitude < 26)
        {
            GetComponent<Renderer>().enabled = false;
        }
        else
        {
            GetComponent<Renderer>().enabled = true;
        }
	}
}
