using System;
using System.Collections.Generic;
using KS.Reactor;
using KS.Reactor.Server;

/*
 * Recieves and sends chat and other informative messages out to clients.
 */
public class Chat : ksServerRoomScript
{
    private readonly string[] KILL_SINGLE =
    {
        "<P> utterly annihilated <S>",
        "<P> made an example of <S>",
        "<P> just upset the scrub <S>",
        "<P> sadistically sizzled <S>",
        "<P> just #rekt <S>",
        "<P> destroyed the n00b <S>",
        "<P> vaporized <S>",
        "<P> expertly eliminated <S>",
        "<P> taught a lesson to <S>",
        "<P> destroyed the menace <S>"
    };

    private readonly string[] KILL_MULTIPLE =
    {
        "<P> utterly annihilated <S>",
        "<P> made examples of <S>",
        "<P> just upset the scrubs <S>",
        "<P> sadistically sizzled <S>",
        "<P> just #rekt <S>",
        "<P> destroyed the n00bs <S>",
        "<P> vaporized <S>",
        "<P> expertly eliminated <S>",
        "<P> taught a lesson to <S>",
        "<P> destroyed the duo <S>"
    };

    private readonly string[] COLLISION =
    {
        "<P> was not looking ahead",
        "<P> still has a learner's licence",
        "<P> doesn't know how to steer",
        "<P> crashed an expensive fighter",
        "<P> learned not to speed",
        "<P> never saw that wall approaching"
    };

    private readonly string[] FIGHTER_COLLISION =
    {
        "<P> got a little too close to <S>",
        "<P> flew into <S>",
        "<P> perished along with <S> in a fiery accident"
    };

    private readonly string[] OUT_OF_BOUNDS =
    {
        "<P> bravely flew away",
        "<P> retreated heroically",
        "<P> abandoned their comrades",
        "<P> won a medal for cowardice"
    };

    private Random m_random = new Random();

    /*
     * Sends out a message to all players.
     */
    public void BroadcastMessage(string message)
    {
        Room.CallRPC(ID.RPC.CHAT_TO_CLIENT, message);
    }

    public void BroadcastJoin(Player player)
    {
        string message = WrapColor(player.PlayerName, player.Color) + " has joined the game.";
        Room.CallRPC(ID.RPC.CHAT_TO_CLIENT, message);
    }

    public void BroadcastLeave(Player player)
    {
        string message = WrapColor(player.PlayerName, player.Color) + " has left the game.";
        Room.CallRPC(ID.RPC.CHAT_TO_CLIENT, message);
    }

    /*
     * Recieves chat messages from the client and transmits it out to the others after filtering.
     */
    [ksRPC(ID.RPC.CHAT_TO_SERVER)]
    public void BroadcastChat(ksIServerPlayer player, string message)
    {
        message = message.Trim();

        if (message.Equals(""))
        {
            return;
        }

        SendChat(message, player.Scripts.Get<Player>());
    }

    /*
     * Sends out a message to the players after formatting it correctly.
     */
    public void SendChat(string message, Player player)
    {
        string chat = WrapColor("<" + player.PlayerName + "> ", player.Color) + message;

        Room.CallRPC(ID.RPC.CHAT_TO_CLIENT, chat);
    }

    /*
     * Sends out a message informing of which player killed who.
     */
    public void BroadcastKill(Player killer, Player[] killed)
    {
        if (killer == null || killed == null)
        {
            return;
        }

        string message = (killed.Length == 1) ? KILL_SINGLE[m_random.Next(KILL_SINGLE.Length)] : KILL_MULTIPLE[m_random.Next(KILL_MULTIPLE.Length)];
        message = message.Replace("<P>", WrapColor(killer.PlayerName, killer.Color));
        message = message.Replace("<S>", GetNames(killed));
        Room.CallRPC(ID.RPC.CHAT_TO_CLIENT, message);
    }

    /*
     * Sends out a message informing players of a player who died in a collision.
     */
    public void BroadcastFighterCollision(Player[] fighterOneKilled, Player[] fighterTwoKilled)
    {
        if (fighterOneKilled == null || fighterTwoKilled == null)
        {
            return;
        }

        string message = FIGHTER_COLLISION[m_random.Next(FIGHTER_COLLISION.Length)];
        message = message.Replace("<P>", GetNames(fighterOneKilled));
        message = message.Replace("<S>", GetNames(fighterTwoKilled));
        Room.CallRPC(ID.RPC.CHAT_TO_CLIENT, message);
    }

    /*
     * Sends out a message informing players of a player who died in a collision.
     */
    public void BroadcastCollision(Player[] killed)
    {
        if (killed == null)
        {
            return;
        }

        string message = COLLISION[m_random.Next(COLLISION.Length)];
        message = message.Replace("<P>", WrapColor(killed[0].PlayerName, killed[0].Color));
        Room.CallRPC(ID.RPC.CHAT_TO_CLIENT, message);
    }

    /*
     * Sends out a message informing players of a player who died out of bounds.
     */
    public void BroadcastOutOfBounds(Player[] killed)
    {
        if (killed == null)
        {
            return;
        }

        string message = OUT_OF_BOUNDS[m_random.Next(OUT_OF_BOUNDS.Length)];
        message = message.Replace("<P>", GetNames(killed));
        Room.CallRPC(ID.RPC.CHAT_TO_CLIENT, message);
    }

    /*
     * Wraps the players with their color and inserts words between the names.
     */
    private string GetNames(Player[] players)
    {
        if (players != null)
        {
            string names = WrapColor(players[0].PlayerName, players[0].Color);

            if (players.Length == 2)
            {
                names += " and " + WrapColor(players[1].PlayerName, players[1].Color);
            }
            return names;
        }
        return "AI Player";
    }

    /*
     * Puts rich text color tags on either side of some text.
     */
    private string WrapColor(string str, ksColor color)
    {
        return "<color=" + color.ToHexString() + ">" + str + "</color>";
    }
}