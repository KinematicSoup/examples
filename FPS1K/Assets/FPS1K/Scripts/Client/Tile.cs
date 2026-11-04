using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach this to an entity prefab to mark it as one of the tiles that can be used in the tile grid to randomly
// generate the level. An editor script looks for these when building configs to build the list of all tile entity
// prefabs the level generator uses.
public class Tile : MonoBehaviour
{
    // Empty start method to allow the script to be disabled in the inspector
    public void Start()
    {
        
    }
}
