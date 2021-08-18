using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }

    public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();

    public delegate void PacketHandler(int fromClient, Packet packet);

    public static Dictionary<int, PacketHandler> packetHandlers;


    private static TcpListener _tcpListener;
    private static UdpClient _udpListener;

    public static void Start(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;

        Debug.Log($"\nServer is starting...");

        InitializeServerData();

        _tcpListener = new TcpListener(IPAddress.Any, Port);

        _tcpListener.Start();

        _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

        _udpListener = new UdpClient(Port);

        _udpListener.BeginReceive(UdpReceiveCallback, null);


        Debug.Log($"Server is starting on {Port}");
    }

    private static void UdpReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);
            _udpListener.BeginReceive(UdpReceiveCallback, null);

            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt();

                if (clientId == 0)
                {
                    return;
                }

                if (Clients[clientId].udp.EndPoint == null)
                {
                    Clients[clientId].udp.Connect(clientEndPoint);
                    return;
                }

                if (Clients[clientId].udp.EndPoint.ToString() == clientEndPoint.ToString())
                {
                    Clients[clientId].udp.HandleData(packet);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error from UDP Receieve callback ErrorCode: {e}");
        }
    }

    public static void SendUdpData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint != null)
            {
                _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error when sending data from UDP ErrorCode: {e}");
        }
    }

    private static void TcpConnectCallback(IAsyncResult result)
    {
        TcpClient client = _tcpListener.EndAcceptTcpClient(result);

        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

        Debug.Log($"{client.Client.RemoteEndPoint} from  incomming connection request...");

        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (Clients[i].tcp.Socket == null)
            {
                Clients[i].tcp.Connect(client);
                return;
            }
        }

        Debug.Log($"{client.Client.RemoteEndPoint} is not connected to server. Server is FULL!");
    }

    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            Clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int) ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived},
            {(int) ClientPackets.playerMovement, ServerHandle.PlayerMovement}
        };

        Debug.Log("Initialized packets.");
    }

    public static void Stop()
    {
        _tcpListener.Stop();
        _udpListener.Close();
    }
}