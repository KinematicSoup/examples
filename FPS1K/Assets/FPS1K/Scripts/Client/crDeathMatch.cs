using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Controls the UI for a death match game.
public class crDeathMatch : ksRoomScript
{
    private bool m_gameOver = true;

    // Called after properties are initialized.
    public override void Initialize()
    {
        DeathMatchGUI.Instance.gameObject.SetActive(true);
        Room.LocalPlayer.OnPropertyChange[Prop.SCORE] += OnScoreChange;
        OnScoreChange(0, Room.LocalPlayer.Properties[Prop.SCORE]);
    }

    // Called when the script is detached.
    public override void Detached()
    {
        if (DeathMatchGUI.Instance != null)
        {
            DeathMatchGUI.Instance.gameObject.SetActive(false);
        }
        if (Room.LocalPlayer != null)
        {
            Room.LocalPlayer.OnPropertyChange[Prop.SCORE] -= OnScoreChange;
        }
    }

    private void Update()
    {
        if (Room.LocalPlayer == null)
        {
            return;
        }
        // Calculate the local player's rank and the 1st place score/username.
        int rank = 1;
        int best = int.MinValue;
        string leader = null;
        int score = Room.LocalPlayer.Properties[Prop.SCORE];
        for (int i = 0; i < Room.Players.Count; i++)
        {
            ksPlayer player = Room.Players[i];
            int s = player.Properties[Prop.SCORE];
            if (s > score)
            {
                rank++;
            }
            if (s > best)
            {
                best = s;
                leader = player.Properties[Prop.USERNAME];
            }
            else if (s == best)
            {
                leader = null;
            }
        }
        DeathMatchGUI.Instance.SetRank(rank, Room.Players.Count);
        DeathMatchGUI.Instance.SetLeader(best, leader);
        DeathMatchGUI.Instance.UpdateScoreColor(score, rank);

        bool gameOver = Room.Properties[Prop.GAME_OVER];
        if (gameOver != m_gameOver)
        {
            m_gameOver = gameOver;
            if (gameOver)
            {
                Hud.Instance.Winner.text = GetWinnerText(best);
                if (rank != 1)
                {
                    Hud.Instance.Winner.text += "\n You placed " + DeathMatchGUI.Instance.RankToString(rank) +
                        " out of " + Room.Players.Count;
                }
            }
            else
            {
                Hud.Instance.Winner.text = "";
            }
        }
    }

    private void OnScoreChange(ksMultiType oldValue, ksMultiType newValue)
    {
        DeathMatchGUI.Instance.Score.text = newValue;
    }

    private string GetWinnerText(int bestScore)
    {
        List<ksPlayer> winners = new List<ksPlayer>();
        for (int i = 0; i < Room.Players.Count; i++)
        {
            ksPlayer player = Room.Players[i];
            if (player.Properties[Prop.SCORE] == bestScore)
            {
                // Always diplay the local player's name first if there is a tie.
                if (player.IsLocal)
                {
                    winners.Insert(0, player);
                }
                else
                {
                    winners.Add(player);
                }
            }
        }
        // Display the names of up to 5 winners who tied.
        if (winners.Count > 5)
        {
            if (winners[0].IsLocal)
            {
                return "You and " + (winners.Count - 1) + " Other Players Tied for 1st!";
            }
            return winners.Count + " Players Tied for 1st!";
        }
        if (winners.Count == 1)
        {
            if (winners[0].IsLocal)
            {
                return "You Won!";
            }
            return winners[0].Properties[Prop.USERNAME] + " Won!";
        }
        string str = winners[0].IsLocal ? "You" : winners[0].Properties[Prop.USERNAME];
        for (int i = 1; i < winners.Count; i++)
        {
            if (i < winners.Count - 1)
            {
                str += ", ";
            }
            else
            {
                str += " and ";
            }
            str += winners[i].Properties[Prop.USERNAME];
        }
        return str + " Tied for 1st!";
    }
}