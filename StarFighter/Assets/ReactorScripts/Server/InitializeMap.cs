using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KS.Reactor;
using KS.Reactor.Server;

/*
 * Randomly spawns asteroids and dust clouds as the room starts up.
 */
public class InitializeMap : ksServerRoomScript
{
    [ksEditable]
    private String[] m_asteroidPrefabs =
    {
        "asteroid1_1",
        "asteroid2_1",
        "asteroid3_1",
        "asteroid4_1",
        "asteroid5_1",
        "asteroid6_1",
        "asteroid7_1",
        "asteroid8_1"
    };

    [ksEditable]
    private float[] m_asteroidScales = { 0.6f, 1.0f, 2.0f, 4.0f };

    [ksEditable]
    private int m_numAsteroids = 400;

    // minimim distance from the center of the map
    [ksEditable]
    private float m_asteroidPlacementMinorRadius = 42.5f;

    // maximum radius from the center of the map
    [ksEditable]
    private float m_asteroidPlacementMajorRadius = 100.0f;

    [ksEditable]
    private int m_asteroidClumps = 7;

    // chance that an asteroid will spawn near a clump
    [ksEditable]
    private float m_asteroidClumpChance = 0.70f;

    // radius of a clump
    [ksEditable]
    private float m_asteroidClumpRadius = 13.0f;

    [ksEditable]
    private float m_minAsteroidVelocity = 0.0f;

    [ksEditable]
    private float m_maxAsteroidVelocity = 0.10f;

    [ksEditable]
    private float m_minAsteroidAngularVelocity = 0.5f;

    [ksEditable]
    private float m_maxAsteroidAngularVelocity = 6.0f;

    [ksEditable]
    private float m_asteroidBaseMass = 50.0f;

    // percent chance to spawn a very large asteroid (last element in asteroidSize array)
    [ksEditable]
    private float m_largeAsteroidChance = 0.05f;

    [ksEditable]
    private string[] m_cloudPrefabs =
    {
        "dustCloud1",
        "dustCloud2",
        "dustCloud3"
    };

    [ksEditable]
    private int m_cloudAmount = 8;

    // minimim distance from the center of the map
    [ksEditable]
    private float m_cloudPlacementMinorRadius = 50.0f;

    // maximum radius from the center of the map
    [ksEditable]
    private float m_cloudPlacementMajorRadius = 100.0f;

    private List<ksSpawnParams> m_spawnList = new List<ksSpawnParams>();

    /**
     * Called when the script is attached.
     */
    public override void Initialize()
    {
        // Record entity types and transforms for initial dynamic entities which will be used to reset the map.
        foreach (ksIServerEntity entity in Room.DynamicEntities)
        {
            m_spawnList.Add(new ksSpawnParams(entity.Type)
            {
                Transform = new ksTransformState(entity.Transform)
            });
        }
        SpawnRandomizedEntities();
    }

    public void Reset()
    {
        foreach (ksIServerEntity entity in Room.DynamicEntities)
        {
            entity.Destroy();
        }
        foreach (ksSpawnParams spawn in m_spawnList)
        {
            Room.SpawnEntity(spawn);
        }
        SpawnRandomizedEntities();
    }

    private void SpawnRandomizedEntities()
    {
        List<ksVector3> positions = new List<ksVector3>();

        ksRandom random = new ksRandom();

        // asteroid spawn
        ksLog.Debug("Spawning " + m_numAsteroids + " asteroids");
        ksSphere sphere = new ksSphere(0);
        ksOverlapParams overlapParams = new ksOverlapParams()
        {
            Shape = sphere
        };
        for (int i = 0; i < m_numAsteroids; i++)
        {
            // pick an asteroid
            string asteroid = m_asteroidPrefabs[random.Next(0, m_asteroidPrefabs.Length)];

            // pick either a random small size or a large size
            int size;

            if (random.NextFloat() > m_largeAsteroidChance)
            {
                size = random.Next(1, m_asteroidScales.Length);
            }
            else
            {
                size = m_asteroidScales.Length;
            }

            // gets the approximate raduis of the different sizes of asteroids, used for avoiding overlap in placement
            float radius;

            switch (size)
            {
                case 1: radius = 1.25f; break;
                case 2: radius = 2.25f; break;
                case 3: radius = 3.75f; break;
                case 4: radius = 8.25f; break;
                default: radius = 1.25f; break;
            }

            // pick a random position, and if the spot overlaps with something, pick another position until a valid one is found.
            // also has a increasing chance to be repositioned if it is close to the equator
            ksVector3 position;

            while (true)
            {
                // place the asteroid in a clump or just randomly
                if (positions.Count > m_asteroidClumps && random.NextFloat() < m_asteroidClumpChance)
                {
                    ksVector3 rndPos = random.NextVector3(2, (float)Math.Sqrt(m_asteroidClumpRadius));

                    position = rndPos * rndPos.MagnitudeSquared() + positions[random.Next(0, m_asteroidClumps)];
                    if (position.Magnitude() >= m_asteroidPlacementMajorRadius || position.Magnitude() <= m_asteroidPlacementMinorRadius)
                    {
                        continue;
                    }
                }
                else
                {
                    position = random.NextVector3(m_asteroidPlacementMinorRadius, m_asteroidPlacementMajorRadius);

                    float relativeY = Math.Abs(position.Y / m_asteroidPlacementMajorRadius);

                    if (random.NextFloat(0, 1) > relativeY * 0.75f + 0.25f)
                    {
                        continue;
                    }
                }

                sphere.Radius = radius;
                overlapParams.Origin = position;
                if (!Physics.OverlapAny(overlapParams))
                {
                    break;
                }
            }

            positions.Add(position);

            ksVector3 scale = ksVector3.One * m_asteroidScales[size - 1];

            ksIServerEntity entity = Room.SpawnEntity(asteroid, position, random.NextQuaternion(), scale);
            ksRigidBody rigidBody = entity.Scripts.Get<ksRigidBody>();
            rigidBody.Mass = m_asteroidBaseMass * (float)Math.Pow(size, 3);
            rigidBody.Velocity = random.NextVector3(m_minAsteroidVelocity, m_maxAsteroidVelocity);
            rigidBody.AngularVelocity = random.NextVector3(m_minAsteroidAngularVelocity, m_maxAsteroidAngularVelocity);
        }

        // dust cloud placement
        for (int i = 0; i < m_cloudAmount; i++)
        {
            string cloud = m_cloudPrefabs[random.Next(0, m_cloudPrefabs.Length)];

            ksVector3 position = random.NextVector3(m_cloudPlacementMinorRadius, m_cloudPlacementMajorRadius);

            Room.SpawnEntity(cloud, position, random.NextQuaternion(), ksVector3.One * 2);
        }
    }
}