using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class DomeUVScroll : MonoBehaviour
{
    public Vector2 UvAnimationRate = new Vector2( 0.015f, 0.015f );
    public string TextureName = "_BumpMap";
 
    Vector2 m_uvOffset = Vector2.zero;
 
    void LateUpdate()
    {
        m_uvOffset += (UvAnimationRate * Time.deltaTime);

        if (GetComponent<Renderer>().enabled)
        {
            GetComponent<Renderer>().materials[0].SetTextureOffset(TextureName, m_uvOffset);
        }
    }
}