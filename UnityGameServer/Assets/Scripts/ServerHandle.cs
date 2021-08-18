using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int fromClient,Packet packet)
    {
        int _clientIdCheck = packet.ReadInt();
        string _username = packet.ReadString();

        Debug.Log($"{Server.Clients[fromClient].tcp.Socket.Client.RemoteEndPoint} connected successfully and PlayerId : {fromClient}");

        if(fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID:{ fromClient} is Contains wrong client id. Client ID: ({_clientIdCheck }) ");
        }

        Server.Clients[fromClient].SendIntoGame(_username);
    }

      

    public static void PlayerMovement(int fromClient, Packet packet)
    {
        bool[] inputs = new bool[packet.ReadInt()];

        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = packet.ReadBool();
        }

        Quaternion rotation = packet.ReadQuaternion();

        Server.Clients[fromClient].Player.SetInput(inputs, rotation);
        
        Debug.Log("Input received from client successfully.");
         
    }
}
