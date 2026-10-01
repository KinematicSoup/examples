using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Controls game UI
public class crHUD : ksRoomScript
{
    // The color of damage text for other players.
    public Color DamageColor;

    private float m_roundTime;
    private bool m_gameOver = true;

    // Called after properties are initialized.
    public override void Initialize()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Hud.Instance.Minimap.CreateGrid(Properties[Prop.SIZE]);
        Hud.Instance.Settings.OnClose += LockCursor;
    }

    public override void Detached()
    {
        Hud.Instance.Settings.OnClose -= LockCursor;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Update()
    {
        // Open the settings menu when escape, enter, or tab is pressed.
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject settingsObj = Hud.Instance.Settings.gameObject;
            settingsObj.SetActive(!settingsObj.activeSelf);
            Cursor.lockState = settingsObj.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (Time.ProcessedServerUnscaledDelta == 0f)
        {
            return;
        }
        // Update the round timer.
        m_roundTime -= Time.ProcessedServerUnscaledDelta;
        if (Room.Properties[Prop.GAME_OVER])
        {
            if (!m_gameOver)
            {
                m_gameOver = true;
                Hud.Instance.SetTime(0f);
                Hud.Instance.ShowDeathScreen();
                Hud.Instance.Minimap.Clear();
            }
        }
        else
        {
            Hud.Instance.SetTime(m_roundTime);
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    [ksRPC(RPC.ROUND_TIME)]
    private void SetRoundTime(float time)
    {
        // We add the amount of unscaled processed server time to the round time because it will get subtracted off in
        // the next Update to get to the initial round time.
        m_roundTime = time + Time.ProcessedServerUnscaledDelta;
        m_gameOver = time <= 0f;
    }

    // Called when the local player damages another player.
    [ksRPC(RPC.DAMAGE)]
    private void ShowDamage(float damage, Vector3 position)
    {
        Hud.Instance.TextOverlay.Add(damage.ToString(), position, DamageColor);
    }

    // Called when the local player kills another player.
    [ksRPC(RPC.KILL)]
    private void Kill(uint playerId)
    {
        ksPlayer player = Room.GetPlayer(playerId);
        if (player != null)
        {
            Hud.Instance.Notifications.Add("You killed " + player.Properties[Prop.USERNAME]);
        }
    }

    // Called when the local player is killed by another player.
    [ksRPC(RPC.KILLED)]
    private void KilledBy(uint playerId)
    {
        ksPlayer player = Room.GetPlayer(playerId);
        if (player == null)
        {
            Hud.Instance.Notifications.Add("You died", Color.red);
        }
        else
        {
            Hud.Instance.Notifications.Add("You were killed by " + player.Properties[Prop.USERNAME], Color.red);
        }
    }
}