using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Generates a random level by creating a grid of random tiles.
public class srLevelGenerator : ksServerRoomScript
{
    // The prefabs that can be used as tiles. This should not be edited in the inspector; it is filled automatically by
    // an editor script that looks for prefab entities with a Tile script attached when configs are built.
    [ksEditable]
    [ksUnityTag("[ksReadOnly]")]
    public string[] TilePrefabs;

    // The number of rows/columns in the grid.
    // For 1000 players we probably want 14 or 15.
    [ksEditable]
    public int GridSize = 3;

    // Use a non-zero seed to force the same level to be generated every time.
    [ksEditable]
    public int Seed = 0;

    // If false, all dynamic rigidbody cubes will be removed from the level, and the player and platforms will be
    // static actors instead of kinematic rigidbodies. This helps improve performance which can help with large
    // numbers of players.
    [ksEditable]
    public bool IncludeDynamicRigidbodies = false;

    public int NumTiles
    {
        get { return GridSize * GridSize; }
    }

    public event Action OnGenerateLevel;

    private ksRandom m_rand;

    // Called when the script is attached.
    public override void Initialize()
    {
        if (Seed == 0)
        {
            m_rand = new ksRandom();
        }
        Properties[Prop.SIZE] = GridSize;
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        if (TilePrefabs == null || TilePrefabs.Length == 0)
        {
            return;
        }
        if (Seed != 0)
        {
            m_rand = new ksRandom(Seed);
        }
        int oldEntities = Room.DynamicEntities.Count;
        foreach (ksIServerEntity entity in Room.DynamicEntities)
        {
            entity.Destroy();
        }
        ksQuaternion[] rotations = new ksQuaternion[4];
        rotations[0] = ksQuaternion.Identity;
        rotations[1] = ksQuaternion.FromAxisAngle(ksVector3.Up, 90f);
        rotations[2] = ksQuaternion.FromAxisAngle(ksVector3.Up, 180f);
        rotations[3] = ksQuaternion.FromAxisAngle(ksVector3.Up, 270f);
        for (int x = 0; x < GridSize; x++)
        {
            for (int z = 0; z < GridSize; z++)
            {
                ksVector3 position = new ksVector3(x * World.TILE_SIZE, 0f, z * World.TILE_SIZE);
                ksQuaternion rotation = rotations[m_rand.Next(rotations.Length)];
                List<ksIServerEntity> entities = 
                    Room.SpawnCollection(TilePrefabs[m_rand.Next(TilePrefabs.Length)], position, rotation);
                if (!IncludeDynamicRigidbodies)
                {
                    // Destroy dynamic rigid bodies and detach kinematic rigid bodies.
                    for (int i = 0; i < entities.Count; i++)
                    {
                        ksIServerEntity entity = entities[i];
                        ksRigidBody rigidBody = entity.Scripts.Get<ksRigidBody>();
                        if (rigidBody == null)
                        {
                            continue;
                        }
                        if (rigidBody.IsKinematic)
                        {
                            entity.Scripts.Detach(rigidBody);
                        }
                        else
                        {
                            entity.Destroy();
                        }
                    }
                }
            }
        }
        if (OnGenerateLevel != null)
        {
            OnGenerateLevel();
        }
        // Rebuilding the dynamic tree after regenerating the level speeds up scene queries.
        Room.PhysXScenes[0].ForceDynamicTreeRebuild();
        ksLog.Info("Entities: " + (Room.DynamicEntities.Count - oldEntities));
    }
}