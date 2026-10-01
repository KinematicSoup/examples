using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// GUI elements specific to death matches. Currently this is the only game type.
public class DeathMatchGUI : MonoBehaviour
{
    // The local player's score.
    public TMP_Text Score;
    // The local player's rank.
    public TMP_Text Rank;
    // The 1st place player's name and score.
    public TMP_Text Leader;
    // The text color for 1st place.
    public Color FirstColor;

    public static DeathMatchGUI Instance;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
        Leader.color = FirstColor;
    }

    public void SetRank(int rank, int count)
    {
        Rank.text = RankToString(rank) + " / " + count;
        Rank.color = rank == 1 ? FirstColor : Color.white;
    }

    public void SetLeader(int score, string username)
    {
        Leader.text = "1st: " + score;
        if (!string.IsNullOrEmpty(username))
        {
            Leader.text += " - " + username;
        }
    }

    public void UpdateScoreColor(int score, int rank)
    {
        if (score < 0)
        {
            Score.color = Color.red;
            return;
        }
        Score.color = rank == 1 ? FirstColor : Color.white;
    }

    public string RankToString(int rank)
    {
        string text = rank.ToString();
        rank %= 100;
        switch (rank >= 20 ? rank % 10 : rank)
        {
            case 1:
            {
                text += "st";
                break;
            }
            case 2:
            {
                text += "nd";
                break;
            }
            case 3:
            {
                text += "rd";
                break;
            }
            default:
            {
                text += "th";
                break;
            }
        }
        return text;
    }
}
