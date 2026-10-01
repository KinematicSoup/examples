using System;
using System.Collections.Generic;
using KS.Reactor;
using KS.Reactor.Server;

/*
 * Controls aspects of a player specific to playing a turret.
 */
public class TurretPlayer : ksServerPlayerScript
{
    private GameManager m_gameManager;

    // which fighter this player is controlling the turret of
    private ksIServerEntity m_fighter;
    public ksIServerEntity Fighter
    {
        get { return m_fighter; }
    }


    public bool Spawned
    {
        get { return m_fighter != null; }
    }

    public override void Initialize()
    {
        m_gameManager = Room.Scripts.Get<GameManager>();
    }

    /*
     * Called when the turret player leaves, but the figter remains.
     */
    public void PlayerLeave()
    {
        if (m_fighter != null)
        {
            m_fighter.Scripts.Get<Turret>().ResetTurret();
        }
    }

    /*
     * Called when the fighter the turret is connected to is destroyed.
     */
    public void TurretDestroyed(bool losePoints)
    {
        m_fighter = null;

        if (losePoints)
        {
            Player.Scripts.Get<Player>().AddDeath();
        }
    }

    /*
     * Returns the main player script.
     */
    public Player GetPlayer()
    {
        return Player.Scripts.Get<Player>();
    }

    /*
     * Attempts to find an avaliable turret on the player's team and if so controls it.
     */
    public void FindTurret()
    {
        m_fighter = m_gameManager.FindTurret(Player);

        if (m_fighter != null)
        {
            m_fighter.Scripts.Get<Turret>().InitializeTurret(this);
        }
    }
}