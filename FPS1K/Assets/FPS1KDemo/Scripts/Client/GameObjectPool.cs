using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A pool of reusable game objects. Pooled objects are stored as children.
public class GameObjectPool : MonoBehaviour
{
    // The object to create instances from.
    public GameObject Template;

    public void Awake()
    {
        // Disable the game object so pooled children are disabled.
        gameObject.SetActive(false);
    }

    // Fetch an instance from the pool, or create one if the pool is empty.
    public GameObject Fetch()
    {
        if (transform.childCount > 0)
        {
            Transform t = transform.GetChild(transform.childCount - 1);
            t.SetParent(null);
            return t.gameObject;
        }
        if (Template == null)
        {
            return null;
        }
        GameObject gameObj = Instantiate(Template);
        return gameObj;
    }

    // Return an instance to the pool.
    public void Return(GameObject gameObj)
    {
        gameObj.transform.SetParent(transform);
    }
}
