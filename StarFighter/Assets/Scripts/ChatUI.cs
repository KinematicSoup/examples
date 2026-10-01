using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour 
{
    public InputField ChatTextField;
    public Text ChatPlaceholdText;
    public Text ChatText;
    public RectTransform ChatBox;
    public GUISkin ChatGuiSkin;

    [Tooltip("How long in seconds a chat message remains fully visible.")]
    [Range(0, 60)]
    public float MessageVisibleTime = 5.0f;

    [Tooltip("Time in seconds a chat message takes to fade out.")]
    [Range(0, 10)]
    public float MessageFadeTime = 5.0f;

    [Tooltip("Font size of chat messages.")]
    [Range(6, 20)]
    public float MessageFontSize = 12;

    private ServerConnection m_connection;
    private ScoreboardUI m_scoreboard;
    private Canvas m_canvas;

    private bool m_chatActive = false;
    public bool ChatActive
    {
        get { return m_chatActive; }
    }

    private bool m_cancelledChat = false;
    public bool CancelledChat
    {
        get { return m_cancelledChat; }
    }

    private ArrayList m_messages = new ArrayList();
    private bool m_openChat = false;
    private bool m_sendMessage = false;
    private bool m_cancel = false;

    void Start()
    {
        GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
        m_connection = gameController.GetComponent<ServerConnection>();
        m_scoreboard = GetComponent<ScoreboardUI>();
        m_canvas = GetComponent<Canvas>();
    }

    public void AddMessage(ChatMessage incomingMessage)
    {
        m_messages.Insert(0, incomingMessage);
    }

    /*
     * Deals with chat related user input.
     */
    void LateUpdate()
    {
        m_cancelledChat = false;

        if (LevelManager.IsLoading() || !m_connection.IsConnected() || m_connection.GetLocalPlayer() == null ||
            m_connection.PlayerName() == null || m_scoreboard.ShowScores)
        {
            SetChatVisible(false);
            return;
        }

        // handle chat input
        if (Controls.ButtonDown(GameButton.CHAT) && !m_chatActive)
        {
            m_openChat = true;
        }
        else if (Controls.ButtonDown(GameButton.CHAT) && m_chatActive)
        {
            m_sendMessage = true;
        }
        else if (Controls.ButtonDown(GameButton.CLOSE_CHAT) && m_chatActive)
        {
            m_cancel = true;
        }

        if (m_openChat) // opens chat
        {
            m_chatActive = true;
            SetChatVisible(true);
            ChatTextField.Select();
            ChatTextField.ActivateInputField();
        }
        else if (m_sendMessage && ChatTextField.text != "") // send message
        {
            m_connection.Room.GameObject.GetComponent<ChatClient>().SendChatMessage(ChatTextField.text);
            ChatTextField.text = "";
            SetChatVisible(false);
            m_chatActive = false;
        }
        else if (m_sendMessage && ChatTextField.text == "") // reselects chat if the user tries to send an empty message
        {
            ChatTextField.Select();
            ChatTextField.ActivateInputField();
        }
        else if (m_cancel) // close chat box
        {
            ChatTextField.text = "";
            SetChatVisible(false);
            m_chatActive = false;
            m_cancelledChat = true;
        }

        m_openChat = false;
        m_sendMessage = false;
        m_cancel = false;
    }

    private void SetChatVisible(bool setVisible)
    {
        ChatTextField.enabled = setVisible;
        ChatTextField.GetComponent<Image>().enabled = setVisible;
        ChatPlaceholdText.enabled = setVisible;
        ChatText.enabled = setVisible;
    }
	
    /*
     * Draws the chat text onto the screen.
     */
    public void OnGUI()
    {
        if (LevelManager.IsLoading() || !m_connection.IsConnected() || m_connection.GetLocalPlayer() == null ||
            m_connection.PlayerName() == null || m_scoreboard.ShowScores)
        {
            return;
        }

        GUI.skin = ChatGuiSkin;

        Rect chatBoxRect = new Rect(
            ChatBox.anchoredPosition.x * m_canvas.scaleFactor,
            -ChatBox.offsetMax.y * m_canvas.scaleFactor, 
            ChatBox.rect.width * m_canvas.scaleFactor, 
            ChatBox.rect.height * m_canvas.scaleFactor
            );

        GUILayout.BeginArea(chatBoxRect);
        GUILayout.BeginScrollView(new Vector2(0, 0));

        for (int i = 0; i < 20; i++)
        {
            if (i < m_messages.Count)
            {
                ChatMessage chatMessage = (ChatMessage)m_messages[i];
                float messageAge = (float)(System.DateTime.Now - chatMessage.Time).TotalSeconds;

                GUI.skin.label.fontSize = (int)(MessageFontSize * m_canvas.scaleFactor);

                if (messageAge <= (MessageVisibleTime + MessageFadeTime))
                {
                    float a = (MessageFadeTime - Mathf.Max(messageAge - MessageVisibleTime, 0)) / MessageFadeTime;

                    GUI.skin.label.normal.textColor = new Color(1, 1, 1, a);
                    GUILayout.Label(chatMessage.GetMessage(a));
                }
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
