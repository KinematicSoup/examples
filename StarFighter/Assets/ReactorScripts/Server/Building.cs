using System;
using System.Collections.Generic;
using KS.Reactor;
using KS.Reactor.Server;

/*
 * Controls buildings in the city, letting them be destroyed.
 */
public class Building : ksServerEntityScript
{
    // multiplied by the mangnitude of the impulse in a collision when subtracting health from a collision
    private const float IMPULSE_DAMAGE_MULTIPLIER = 3.5f;
    // minimum impulse that must be felt during a collision for the building part to take damamge
    private const float MIN_IMPULSE_DAMAGE = 10.0f;

    [ksEditable]
    private int m_health = 1600;

    public override void Initialize()
    {
        Entity.OnCollision += OnCollision;
    }

    public override void Detached()
    {
        Entity.OnCollision -= OnCollision;
    }

    private void OnCollision(ksContact contact)
    {
        if (contact.Entity1.Type == ID.TYPE.LASER)
        {
            m_health -= contact.Entity1.Scripts.Get<Laser>().Damage;
        }
        else if (contact.Entity1.Type == ID.TYPE.MISSILE)
        {
            m_health -= contact.Entity1.Scripts.Get<Missile>().Damage;
        }
        else
        {
            float damage = (int)(contact.Impulse.Magnitude() * IMPULSE_DAMAGE_MULTIPLIER);

            if (damage > MIN_IMPULSE_DAMAGE)
            {
                m_health -= (int)damage;
            }
        }
        if (m_health <= 0f)
        {
            Entity.Destroy();
        }
    }
}