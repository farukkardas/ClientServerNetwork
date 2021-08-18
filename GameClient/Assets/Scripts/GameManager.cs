using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static  Dictionary<int, PlayerManager> Players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else if (Instance != null)
        {
            Debug.Log($"{Instance} mevcut , obje destroy ediliyor!");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;

        if (_id == Client.Instance.myId)
        {
            _player = Instantiate(localPlayerPrefab,_position,_rotation);
        }

        else
        {
            _player = Instantiate(playerPrefab ,_position, _rotation);
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        Players.Add(_id,_player.GetComponent<PlayerManager>());

    }
}
