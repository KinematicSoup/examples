using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using KS.Reactor;
using KS.Reactor.Client;
using KS.Reactor.Client.Unity;
using static KS.Reactor.Client.ksBaseRoom;

// Connection script
public class Connect : MonoBehaviour
{
    public bool ConnectOnline = false;

    public string Host;
    public ushort Port;

    public int MaxFrameRate = 120;

    private ksRoom m_room;

    // Start is called before the first frame update
    private void Start()
    {
        if (MaxFrameRate > 0)
        {
            Application.targetFrameRate = Mathf.Max(60, MaxFrameRate);
        }

        // Bind Unity input to Reactor input.
        ksReactor.InputManager.BindAxis(Axes.X, "Horizontal");
        ksReactor.InputManager.BindAxis(Axes.Z, "Vertical");
        ksReactor.InputManager.BindButton(Buttons.JUMP, "Jump");
        ksReactor.InputManager.BindButton(Buttons.SHOOT, "Fire1", ValidateMouseInput);
        ksReactor.InputManager.BindButton(Buttons.DASH, "Dash");

        Hud.Instance.ConnectButton.onClick.AddListener(ConnectToServer);
        Hud.Instance.UsernameField.text = Config.Instance.Username;
        Hud.Instance.UsernameField.onSubmit.AddListener(OnSubmit);
        Hud.Instance.UsernameField.ActivateInputField();
    }

    // Prevent shooting when the player is clicking on UI.
    private bool ValidateMouseInput()
    {
        return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
    }

    private void OnSubmit(string username)
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        if (!Hud.Instance.ConnectButton.enabled || m_room != null)
        {
            return;
        }
        Hud.Instance.ConnectButton.enabled = false;

        if (ConnectOnline)
        {
            ksReactor.GetServers(OnGetRooms);
        }
        else
        {
            ksRoomInfo info = GetComponent<ksRoomType>().GetRoomInfo(Host, Port);
            ConnectToServer(info);
        }
    }

    private void OnGetRooms(List<ksRoomInfo> rooms, string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Hud.Instance.ConnectButton.enabled = true;
            ksLog.Error(this, "Error getting rooms: " + error);
            return;
        }
        if (rooms.Count == 0)
        {
            Hud.Instance.ConnectButton.enabled = true;
            ksLog.Warning(this, "No rooms available.");
            return;
        }
        ConnectToServer(rooms[0]);
    }

    private void ConnectToServer(ksRoomInfo roomInfo)
    {
        m_room = new ksRoom(roomInfo);
        m_room.OnConnect += OnConnect;
        m_room.OnDisconnect += OnDisconnect;
        m_room.InputInterval = 0f;// Send inputs every client frame.

        ushort port;
        if (ushort.TryParse(Hud.Instance.PortField.text, out port))
        {
            ksAddress address = roomInfo.GetAddress(m_room.Protocol);
            address.Port = port;
            roomInfo.SetAddress(m_room.Protocol, address);
        }

        string username = Hud.Instance.UsernameField.text;
        Hud.Instance.Username.text = username;
        Config.Instance.Username = username;
        Config.Instance.Save();
        m_room.Connect(username);
    }

    private void OnConnect(ConnectStatus status, ksAuthenticationResult result)
    {
        Hud.Instance.ConnectButton.enabled = true;
        if (status == ksBaseRoom.ConnectStatus.SUCCESS)
        {
            ksLog.Info("Connected to " + m_room);
            Hud.Instance.ShowGameScreen();
        }
        else
        {
            ksLog.Error(this, "Unable to connect to " + m_room + ". Status = " + status + "(" + result + ")");
            m_room.CleanUp();
            m_room = null;
        }
    }

    private void OnDisconnect(ksBaseRoom.ConnectStatus status)
    {
        ksLog.Info("Disconnected from " + m_room + ". Status = " + status);
        m_room.CleanUp();
        m_room = null;
        Hud.Instance.ShowConnectScreen();
    }
}
