using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor;
using KS.Reactor.Client.Unity;

/*
 * Manages player chat messages and notifications from and to the server. This does not interact directly with the ui, 
 * rather it organizes the messages into specific types and gives it to the ChatUI.
 */
public class ChatClient : ksRoomScript
{
    private ChatUI m_chat;

    public override void Initialize()
    {
        m_chat = GameObject.FindGameObjectWithTag("GUI").GetComponent<ChatUI>();
    }

    /*
     * Revieves chat messages and other notifications from the server.
     */
    [ksRPC(ID.RPC.CHAT_TO_CLIENT)]
    private void ReceiveChat(string message)
    {
        m_chat.AddMessage(new ChatMessage(message));
    }

    /*
     * Sends a player chat message to the server.
     */
    public void SendChatMessage(string message)
    {
        Room.CallRPC(ID.RPC.CHAT_TO_SERVER, message);
    }
}


/*
 * Stores a message from the server.
 */
public class ChatMessage
{
    private string m_message;

    private DateTime m_time;
    public DateTime Time
    {
        get { return m_time; }
    }

    public ChatMessage(string message)
    {
        m_message = message;
        m_time = DateTime.Now;
    }

    /*
     * Returns the server message with the alpha set to a specific value. The server sends <a> where we need
     * to insert a hex alpha value, so we just replace the tags with that value.
     */
    public string GetMessage(float alpha)
    {
        byte a = (byte)(alpha * 255);
        return m_message.Replace("<a>", a.ToString("X2"));
    }
}