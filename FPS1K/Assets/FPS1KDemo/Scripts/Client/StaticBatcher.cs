using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Creates a static batch of objects for each group of objects that share a material that were added to this on the
// same frame.
public class StaticBatcher : MonoBehaviour
{
    public static StaticBatcher Instance
    {
        get { return m_instance; }
    }
    private static StaticBatcher m_instance;

    private Dictionary<Material, List<GameObject>> m_batches = new Dictionary<Material, List<GameObject>>();

    // Start is called before the first frame update
    private void Awake()
    {
        m_instance = this;
    }

    // Update is called once per frame
    private void Update()
    {
        // Create the static batches if any objects were added this frame.
        foreach (List<GameObject> objects in m_batches.Values)
        {
            StaticBatchingUtility.Combine(objects.ToArray(), gameObject);
        }
        m_batches.Clear();
    }

    // Adds a game object to be batched with other objects that share the same material that were added on the same
    // frame.
    public void Add(GameObject gameObject)
    {
        if (!enabled)
        {
            return;
        }
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        if (renderer == null || renderer.sharedMaterial == null)
        {
            return;
        }
        List<GameObject> objs;
        if (!m_batches.TryGetValue(renderer.sharedMaterial, out objs))
        {
            objs = new List<GameObject>();
            m_batches[renderer.sharedMaterial] = objs;
        }
        objs.Add(gameObject);
    }
}
