using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using UnityEngine.UI;

public class ScrollContent : MonoBehaviour 
{
    public RectTransform Content;

	void Start () 
    {
        GetComponent<ScrollRect>().content = Content;
	}
}
