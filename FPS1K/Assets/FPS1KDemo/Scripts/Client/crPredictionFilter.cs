using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Disables prediction interpolation for distant entities.
public class crPredictionFilter : ksRoomScript
{
    // Entities further from the camera than this have prediction disabled.
    public float Distance = 40f;

    private List<ksEntity> m_entities = new List<ksEntity>();

    // Called after properties are initialized.
    public override void Initialize()
    {
        Room.OnSpawnEntity += OnSpawnEntity;   
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnSpawnEntity -= OnSpawnEntity;
    }

    // Called every frame.
    private void Update()
    {
        if (UnityEngine.Time.frameCount % 4 != 0)
        {
            return;
        }
        float dist2 = Distance * Distance;
        for (int i = m_entities.Count - 1; i >= 0; i--)
        {
            ksEntity entity = m_entities[i];
            if (entity.IsDestroyed)
            {
                m_entities[i] = m_entities[m_entities.Count - 1];
                m_entities.RemoveAt(m_entities.Count - 1);
                continue;
            }
            entity.PredictionEnabled = 
                (Camera.main.transform.position - entity.ServerTransform.Position).sqrMagnitude <= dist2;
        }
    }

    private void OnSpawnEntity(ksEntity entity)
    {
        // Add non-statically batched entities to the list of entities to enable/disable prediction on.
        if (entity.GameObject.GetComponent<StaticBatched>() == null)
        {
            m_entities.Add(entity);
        }
    }
}