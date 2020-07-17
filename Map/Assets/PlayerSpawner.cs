using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;

    public List<OtherPlayer> Others;
    public List<Client.PlayerData> OthersToSpawn;

    private void Awake()
    {
        Others = new List<OtherPlayer>();
        OthersToSpawn = new List<Client.PlayerData>();
        Client.OnGetOtherCharacters += QueueOthers;
        Client.OnOtherPlayerMoved += MoveOtherPlayer;
    }

    private void OnDestroy()
    {
        Client.OnGetOtherCharacters -= QueueOthers;
        Client.OnOtherPlayerMoved -= MoveOtherPlayer;
    }

    public void MoveOtherPlayer(int id, int x, int y)
    {
        foreach(var other in Others)
        {
            if(other.ID == id)
            {
                other.X = x;
                other.Y = y;
                return;
            }
        }
    }

    public void Update()
    {
        if (OthersToSpawn.Count > 0)
            SpawnOthers();
    }

    public void SpawnOthers()
    {
        foreach (var other in OthersToSpawn)
        {
            var obj = Instantiate(_prefab, new Vector2(other.X, other.Y), Quaternion.identity, transform);
            var otherPlayer = obj.GetComponent<OtherPlayer>();
            otherPlayer.ID = other.ID;
            otherPlayer.X = other.X;
            otherPlayer.Y = other.Y;
            Others.Add(otherPlayer);
        }
        OthersToSpawn = new List<Client.PlayerData>();
    }

    public void QueueOthers(List<Client.PlayerData> others)
    {
        OthersToSpawn = others;
    }
}
