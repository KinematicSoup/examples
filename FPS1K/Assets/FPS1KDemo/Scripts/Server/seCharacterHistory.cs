using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Extends seTransformHistory to also track the history of the head collider's positions.
public class seCharacterHistory : seTransformHistory
{
    private ksSphereCollider m_head;
    private ksVector3[] m_headHistory = new ksVector3[HISTORY_SIZE];
    private ksVector3 m_currentOffset;

    public override void Initialize()
    {
        m_head = Scripts.Get<ksSphereCollider>();
        if (m_head == null)
        {
            Scripts.Detach(this);
            return;
        }
        base.Initialize();
    }

    protected override void StoreHistory(int index)
    {
        base.StoreHistory(index);
        m_headHistory[index] = m_head.Offset;
    }

    protected override void StoreCurrent()
    {
        base.StoreCurrent();
        m_currentOffset = m_head.Offset;
    }

    protected override void RestoreCurrent()
    {
        base.RestoreCurrent();
        m_head.Offset = m_currentOffset;
    }

    protected override ksIServerEntity CreateTempEntity()
    {
        ksIServerEntity tempEntity = base.CreateTempEntity();
        m_head = tempEntity.Scripts.Get<ksSphereCollider>();
        return tempEntity;
    }

    protected override void RewindTo(ksTransform transform, int index, float t)
    {
        base.RewindTo(transform, index, t);
        m_head.Offset = m_headHistory[index];
    }
}