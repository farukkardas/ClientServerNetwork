using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    private static int dataBufferSize = 4096;
    private int id;
    public Player Player;
    public TCP tcp;
    public UDP udp;

    public Client(int clientId)
    {
        id = clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient Socket;
        private NetworkStream _stream;
        private byte[] _receiveBuffer;
        private Packet _receivedData;
        private readonly int _id;

        public TCP(int id)
        {
            this._id = id;
        }

        public void Connect(TcpClient socket)
        {
            Socket = socket;
            Socket.ReceiveBufferSize = dataBufferSize;
            Socket.SendBufferSize = dataBufferSize;

            _stream = Socket.GetStream();

            _receivedData = new Packet();
            _receiveBuffer = new byte[dataBufferSize];

            _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            // send welcome message
            ServerSend.Welcome(_id, " bu mesaj sunucudan gonderilmistir.");
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (Socket != null)
                {
                    _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Sending data sırasında hata yakalandı. Player id {_id} Hata kodu {e}\n");
             
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);

                if (byteLength <= 0)
                {
                    Server.Clients[_id].Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);

                //handle data
                _receivedData.Reset(HandleData(data));
                _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                Debug.Log($"Receive callback sırasında hata yakalandı. Hata kodu {e}\n");

                Server.Clients[_id].Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            _receivedData.SetBytes(data);

            if (_receivedData.UnreadLength() >= 4)
            {
                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
            {
                byte[] packedBytes = _receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packedBytes))
                    {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](_id, packet);
                    }
                });

                packetLength = 0;
                if (_receivedData.UnreadLength() >= 4)
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            Socket.Close();
            _stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            Socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint EndPoint;

        private int _id;

        public UDP(int id)
        {
            this._id = id;
        }

        public void Connect(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public void SendData(Packet packet)
        {
            Server.SendUdpData(EndPoint, packet);
        }

        public void HandleData(Packet packetData)
        {
            int packetLength = packetData.ReadInt();
            byte[] packetBytes = packetData.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();

                    Server.packetHandlers[packetId](_id, packet);
                }
            });
        }

        public void Disconnect()
        {
            EndPoint = null;
        }
    }

    public void SendIntoGame(string playerName)
    {
        Player = NetworkManager.instance.InstantiatePlayer();
        Player.Initialize(id, playerName);


        foreach (Client client in Server.Clients.Values)
        {
            if (client.Player != null)
            {
                if (client.id != id)
                {
                    ServerSend.SpawnPlayer(id, client.Player);
                }
            }
        }

        foreach (Client client in Server.Clients.Values)
        {
            if (client.Player != null)
            {
                ServerSend.SpawnPlayer(client.id, Player);
            }
        }
    }

    public void Disconnect()
    {
        Debug.Log($"{tcp.Socket.Client.RemoteEndPoint} has disconnected");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(Player.gameObject);
            Player = null;

        });
        
      
        
        tcp.Disconnect();
        udp.Disconnect();
        
        ServerSend.PlayerDisconnected(id);
    }
}