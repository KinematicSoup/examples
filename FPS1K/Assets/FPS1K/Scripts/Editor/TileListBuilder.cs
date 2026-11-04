using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client.Unity.Editor;
using KSProxies.Scripts;

// Builds the list of Tile prefabs that the server can spawn when randomly generating a level. Looks for entity prefabs
// in Resources or asset bundles containing an enabled Tile script when building Reactor configs.
[InitializeOnLoad]
public class TileListBuilder
{
    private static string[] m_tilePrefabs;

    static TileListBuilder()
    {
        ksBuildEvents.PreBuild += BuildTileList;
        ksBuildEvents.PreBuildConfig += WriteTileListConfig;
    }

    private static void BuildTileList()
    {
        List<string> prefabs = new List<string>();
        foreach (ksBuildUtils.PrefabInfo info in ksBuildUtils.IterateResourceAndAssetBundlePrefabs())
        {
            Tile tile = info.GameObject.GetComponent<Tile>();
            if (tile != null && tile.enabled)
            {
                prefabs.Add(info.GameObject.name);
            }
        }
        m_tilePrefabs = prefabs.ToArray();
    }

    private static void WriteTileListConfig(Scene scene)
    {
        if (!scene.IsValid())
        {
            return;
        }
        // Look for a root-level object with an srLevelGenerator script to store the tile list in.
        foreach (GameObject gameObject in scene.GetRootGameObjects())
        {
            srLevelGenerator generator = gameObject.GetComponent<srLevelGenerator>();
            if (generator != null)
            {
                generator.TilePrefabs = m_tilePrefabs;
                EditorUtility.SetDirty(generator);
            }
        }
    }
}
