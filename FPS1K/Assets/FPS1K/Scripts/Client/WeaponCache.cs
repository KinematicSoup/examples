using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;
using KS.Reactor.Client;
using KS.Reactor.Client.Unity;
using System.IO;

// Loads and caches weapon prefabs. Each weapon has an sWeapon server script asset and a corresponding prefab to load
// on the client. The sWeapon script assets should be located in Assets/Weapons, and the prefabs should be in
// Assets/Resources/Weapons. The prefab should have the same name as the corresponding sWeapon script asset.
public class WeaponCache
{
    private static Dictionary<uint, GameObject> m_cache = new Dictionary<uint, GameObject>();

    public static GameObject Get(uint assetId)
    {
        GameObject prefab;
        if (m_cache.TryGetValue(assetId, out prefab))
        {
            return prefab == null ? null : GameObject.Instantiate(prefab);
        }
        string path = ksScriptAsset.Assets.GetPath(assetId);
        if (!string.IsNullOrEmpty(path))
        {
            path = "Weapons/" + Path.GetFileName(path);
            prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                m_cache[assetId] = prefab;
                return GameObject.Instantiate(prefab);
            }
            else
            {
                ksLog.Error("Unable to load weapon from " + path);
            }
        }
        else
        {
            ksLog.Error("Unable to load weapon id " + assetId);
        }
        m_cache[assetId] = null;
        return null;
    }
}
