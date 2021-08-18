using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTcpData(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.Tcp.SendData(packet);
    }

    private static void SendUdpData(Packet packet)
    {
        packet.WriteLength();

        Client.Instance.Udp.SendData(packet);
    }

    #region Packets

    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int) ClientPackets.welcomeReceived))
        {
            packet.Write(Client.Instance.myId);
            packet.Write(UIManager.Instance.UsernameField.text);

            SendTcpData(packet);
        }
    }


    public static void PlayerMovement(bool[] inputs)
    {
        using (Packet packet = new Packet((int) ClientPackets.playerMovement))
        {
            packet.Write(inputs.Length);

            foreach (bool input in inputs)
            {
                packet.Write(input);
            }
            
            packet.Write(GameManager.Players[Client.Instance.myId].transform.rotation);
            
            SendUdpData(packet);
        }
    }

    #endregion
}