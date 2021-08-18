using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;


public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string msg = _packet.ReadString();
        int myId = _packet.ReadInt();
        
        Debug.Log($"Sunucudan TCP ile gelen mesaj: {msg}");
        Client.Instance.myId = myId;
        ClientSend.WelcomeReceived();
        
       
        Client.Instance.Udp.Connect(((IPEndPoint)Client.Instance.Tcp.Socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();

        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        
        GameManager.Instance.SpawnPlayer(_id,_username,_position,_rotation);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.Players[_id].transform.position = _position;
    }
    
    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.Players[_id].transform.rotation = _rotation;
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();
        
        Destroy(GameManager.Players[_id].gameObject);
        GameManager.Players.Remove(_id);
    }
}