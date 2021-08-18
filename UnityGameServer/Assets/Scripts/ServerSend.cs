using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{


    private static void SendTcpData(int toClient, Packet packet)
    {
        packet.WriteLength();

        Server.Clients[toClient].tcp.SendData(packet);
    }

    private static void SendTcpDataToAll(Packet packet)
    {
        packet.WriteLength();

        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.Clients[i].tcp.SendData(packet);
        }
    }


    private void SendTcpDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();

        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.Clients[i].tcp.SendData(packet);
            }
        }
    }

    private static void SendUdpData(int toClient, Packet packet)
    {
        packet.WriteLength();

        Server.Clients[toClient].udp.SendData(packet);
    }

    private static void SendUdpDataToAll(Packet packet)
    {
        packet.WriteLength();

        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.Clients[i].udp.SendData(packet);
        }
    }


    public static void SendUdpDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();

        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.Clients[i].udp.SendData(packet);
            }
        }
    }


    #region Packets

    public static void Welcome(int toClient, string msg)
    {
        using (Packet packet = new Packet((int) ServerPackets.welcome))
        {
            packet.Write(msg);
            packet.Write(toClient);

            SendTcpData(toClient, packet);
        }
    }


    public static void SpawnPlayer(int toClient, Player player)
    {
        using (Packet packet = new Packet((int) ServerPackets.spawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTcpData(toClient, packet);
        }
    }


    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="player">The player whose position to update.</param>
    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUdpDataToAll(packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients except to himself (to avoid overwriting the local player's rotation).</summary>
    /// <param name="player">The player whose rotation to update.</param>
    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUdpDataToAll(player.id, packet);
        }
    }

    public static void PlayerDisconnected(int playerId)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerDisconnected))
        {
            packet.Write(playerId);

            SendTcpDataToAll(packet);
        }
    }
    #endregion
}