using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;
using UnityEngine.UI;

public class ScoreboardUI : MonoBehaviour
{
    public RectTransform TeamBoardPrefab;
    public RectTransform PlayerBoardPrefab;
    public RectTransform ScoreBoard;

    public Color PlayerColor;
    public Color OtherPlayersColor;

    private bool m_showScores = false;
    public bool ShowScores
    {
        get { return m_showScores; }
    }

    private GameManagerClient m_gameManager;
    private ServerConnection m_connection;
    private Canvas m_canvas;

    private List<RectTransform> m_teamPanels = new List<RectTransform>();


    void Start()
    {
        m_connection = GameObject.FindGameObjectWithTag("GameController").GetComponent<ServerConnection>();
        m_canvas = GetComponent<Canvas>();
    }

    void Update()
    {
        if (m_gameManager == null && m_connection.IsConnected())
        {
            m_gameManager = m_connection.Room.GameObject.GetComponent<GameManagerClient>();
        }

        if (LevelManager.IsLoading() || !m_connection.IsConnected() || m_connection.GetLocalPlayer() == null || m_connection.PlayerName() == null)
        {
            ScoreBoard.gameObject.SetActive(false);
            return;
        }

        // initailizes the team boards
        int numPanels = Mathf.Max(1, m_gameManager.NumberOfTeams);
        if (m_teamPanels.Count == 0 && m_gameManager != null)
        {
            for (int i = 0; i < numPanels; i++)
            {
                RectTransform panel = Instantiate(TeamBoardPrefab);
                panel.transform.SetParent(ScoreBoard.transform, false);
                panel.transform.localScale = Vector3.one;

                Text team = panel.Find("Name").GetComponent<Text>();
                if (m_gameManager.NumberOfTeams <= 0)
                {
                    team.text = "Scores";
                }
                else
                {
                    team.text = m_gameManager.GetTeamName(i) + "  " + m_gameManager.GetTeamScore(i) + "/" + m_gameManager.GameWinScore;
                    team.color = m_gameManager.GetTeamColor(i);
                }

                m_teamPanels.Add(panel);
            }
        }

        if (Controls.ButtonDown(GameButton.SCORES))
        {
            m_showScores = m_showScores ? false : true;
        }

        if (!m_showScores || LevelManager.IsLoading() || m_gameManager == null)
        {
            ScoreBoard.gameObject.SetActive(false);
            return;
        }

        ScoreBoard.gameObject.SetActive(true);

        // sets the width of the scoreboard
        ScoreBoard.sizeDelta = new Vector2(Mathf.Min(Screen.width / m_canvas.scaleFactor - (100 * 2), 700), 0);

        // updates the scoreboard
        for (int i = 0; i < numPanels; i++)
        {
            // sets the team information
            Text team = m_teamPanels[i].Find("Name").GetComponent<Text>();
            if (m_gameManager.NumberOfTeams > 0)
            {
                team.text = m_gameManager.GetTeamName(i) + "  " + m_gameManager.GetTeamScore(i) + " / " + m_gameManager.GameWinScore;
            }

            Transform teamPanel = m_teamPanels[i].Find("PlayersScroll/Players");
            List<ScoreboardPlayer> players = m_gameManager.GetPlayers(m_gameManager.NumberOfTeams > 0 ? i : -1);

            for (int k = 0; k < Mathf.Max(teamPanel.childCount, players.Count); k++)
            {
                Transform playerPanel = null;

                if (teamPanel.childCount > k && players.Count > k) // we have a player and a panel for their infromation
                {
                    playerPanel = teamPanel.GetChild(k);
                }
                else if (teamPanel.childCount <= k) // we have a player but don't yet have a panel for their infromation, so make one
                {
                    playerPanel = Instantiate(PlayerBoardPrefab);
                    playerPanel.transform.SetParent(teamPanel, false);
                    playerPanel.transform.localScale = Vector3.one;
                }
                else if (players.Count <= k) // we have a panel but there is not player that needs it, so get rid of it
                {
                    Destroy(teamPanel.GetChild(k).gameObject);
                    continue;
                }

                // sets the player information
                ScoreboardPlayer player = (ScoreboardPlayer)players[k];
                Text name = playerPanel.Find("Name").GetComponent<Text>();
                Text type = playerPanel.Find("PlayerType").GetComponent<Text>();
                Text score = playerPanel.Find("Score").GetComponent<Text>();
                Text kills = playerPanel.Find("Kills").GetComponent<Text>();
                Text deaths = playerPanel.Find("Deaths").GetComponent<Text>();

                name.text = player.Name;
                type.text = player.Type;
                score.text = player.Score.ToString();
                kills.text = player.Kills.ToString();
                deaths.text = player.Deaths.ToString();

                Color color;
                // make the local player's info use a different color
                if (player.Name == m_connection.GetLocalPlayer().GameObject.GetComponent<PlayerClient>().PlayerName)
                {
                    color = PlayerColor;
                }
                else
                {
                    color = OtherPlayersColor;
                }

                color.a = player.IsAlive ? 1 : 0.375f;

                name.color = color;
                score.color = color;
                kills.color = color;
                deaths.color = color;
            }
        }
    }
}
