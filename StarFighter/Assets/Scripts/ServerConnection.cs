using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using KS.Reactor;
using KS.Reactor.Client;
using KS.Reactor.Client.Unity;

/*
 * Responsible for initiating and maintaining the connection to the servers.
 */
class ServerConnection : MonoBehaviour
{
    public RectTransform DisconnectDialogue = null;

    public bool ConnectOnline = false;
    public string Host = "localhost";

    private ksRoom m_room = null;
    public ksRoom Room
    {
        get { return m_room; }
    }

    private ksService m_service = null;
    private bool m_backToMenu = false;
    private bool m_requestDisconnect = false;

    /**
     * Attempt to join a server after launching the game scene.
     */
    public void Start()
    {
        m_service = ksReactor.Service;
        if (ConnectOnline)
        {
            ksReactor.GetServers(HandleGetRooms);
        }
        else
        {
            ksRoomInfo info = GetComponent<ksRoomType>().GetRoomInfo(Host, 8000);
            Connect(info);
        }
        ksReactor.GetServers(null);
    }

    private void HandleGetRooms(List<ksRoomInfo> rooms, string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            ksLog.Error(this, "Error getting rooms: " + error);
            m_backToMenu = true;
        }
        else if (rooms.Count == 0)
        {
            ksLog.Warning(this, "No running servers.");
            m_backToMenu = true;
        }
        else
        {
            Connect(rooms[0]);
        }
    }

    private void Connect(ksRoomInfo roomInfo)
    {
        FighterController.InputPredictionEnabled = Settings.UsePrediction;

        m_room = new ksRoom(roomInfo);
        m_room.Connect();

        m_room.OnConnect += OnConnect;
        m_room.OnDisconnect += OnDisconnect;
    }

    /**
     * Reports the success of our connection attempt and take action given a specific result.
     */
    private void OnConnect(ksBaseRoom.ConnectStatus status, ksAuthenticationResult result)
    {
        if (status != ksBaseRoom.ConnectStatus.SUCCESS)
        {
            Debug.Log("Attempt to connect failed! Going back to main menu...");
            m_backToMenu = true;
            return;
        }
        m_room.GameObject.AddComponent<ChatClient>();
        m_room.GameObject.AddComponent<GameManagerClient>();

        // Send player settings to the server.
        Room.CallRPC(ID.RPC.PLAYER_SETTINGS, Settings.PlayerName, Settings.FighterGlow);
    }

    /**
     * If the server disconnects us or we can't find it for whatever reason, report
     * the error to the user and go back to the menu.
     */
    private void OnDisconnect(ksBaseRoom.ConnectStatus status)
    {
        Debug.Log("Connection lost! Going back to main menu...");
        m_backToMenu = true;
    }

    public void Update()
    {
        if (m_backToMenu && !m_requestDisconnect)
        {
            RectTransform dialogue = Instantiate(DisconnectDialogue);
            dialogue.SetParent(GameObject.FindGameObjectWithTag("GUI").transform, false);
            m_backToMenu = false;
        }
    }

    public void Disconnect()
    {
        m_service.Disconnect();
        m_requestDisconnect = true;
    }

    public ksPlayer GetLocalPlayer()
    {
        return m_room == null ? null : m_room.LocalPlayer;
    }

    public bool IsConnected()
    {
        return m_room != null && m_room.IsConnected;
    }

    public bool IsConnecting()
    {
        return m_room != null && m_room.IsConnecting;
    }

    public string PlayerName()
    {
        return GetLocalPlayer().GameObject.GetComponent<PlayerClient>().PlayerName;
    }
}