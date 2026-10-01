using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Controls bot inputs
public class spBot : ksServerPlayerScript
{
    // Minimum time in seconds before choosing the next action.
    [ksEditable]
    public float MinInterval = 4f;
    // Maximum time in seconds before choosing the next action.
    [ksEditable]
    public float MaxInterval = 16f;
    // Chance that the next action is to remain standing idle.
    [ksEditable]
    public float IdleChance = 1f / 3f;
    // Chance that the bot will start shooting.
    [ksEditable]
    public float ShootChance = 1f / 2f;

    private ksRandom m_rand;

    private float m_timer = 0f;
    private float m_jumpTimer = 0f;
    private float m_shootTimer = 0f;

    // Called when the script is attached.
    public override void Initialize()
    {
        if (!Player.IsVirtual)
        {
            Scripts.Detach(this);
            return;
        }
        m_rand = Room.Scripts.Get<srPlayerSpawner>().Rand;
        Room.OnUpdate[0] += Update;
        m_jumpTimer = m_rand.NextFloat(MinInterval, MaxInterval);
        ChooseAction();
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }
    
    // Called during the update cycle
    private void Update()
    {
        m_timer -= Time.Delta;
        m_jumpTimer -= Time.Delta;
        m_shootTimer -= Time.Delta;
        if (m_jumpTimer <= 0f)
        {
            VirtualInput.SetButton(Buttons.JUMP, true);
            m_jumpTimer = m_rand.NextFloat(MinInterval, MaxInterval);
        }
        else
        {
            VirtualInput.SetButton(Buttons.JUMP, false);
        }
        if (m_shootTimer <= 0f)
        {
            VirtualInput.SetButton(Buttons.SHOOT, m_rand.NextFloat() < ShootChance);
            m_shootTimer = m_rand.NextFloat(MinInterval, MaxInterval) * .5f;
        }
        if (m_timer <= 0f)
        {
            ChooseAction();
        }
    }

    private void ChooseAction()
    {
        m_timer = m_rand.NextFloat(MinInterval, MaxInterval);
        if (m_rand.NextFloat() < IdleChance)
        {
            VirtualInput.SetAxis(Axes.Z, 0f);
            return;
        }
        VirtualInput.SetAxis(Axes.Z, 1f);
        VirtualInput.SetAxis(Axes.YAW, m_rand.NextFloat(-1f, 1f));
    }
}