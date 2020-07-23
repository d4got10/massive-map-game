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
        Client.OnGetOtherCharacter += QueueOther;
        Client.OnOtherPlayerMoved += MoveOtherPlayer;
    }

    private void OnDestroy()
    {
        Client.OnGetOtherCharacter -= QueueOther;
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
                other.PositionUpdate = true;
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
        var other = OthersToSpawn[0];
        OthersToSpawn.RemoveAt(0);

        var obj = Instantiate(_prefab, new Vector2(other.X, other.Y), Quaternion.identity, transform);
        obj.name = Others.Count.ToString();
        var otherPlayer = obj.GetComponent<OtherPlayer>();

        otherPlayer.ID = other.ID;
        otherPlayer.X = other.X;
        otherPlayer.Y = other.Y;
        otherPlayer.Name = other.Name;
        otherPlayer.Color = new Color(other.Color.Item1/256f, other.Color.Item2/256f, other.Color.Item3/256f);

        otherPlayer.PositionUpdate = true;
        otherPlayer.Setup = true;

        Others.Add(otherPlayer);  
    }

    public void QueueOther(Client.PlayerData other)
    {
        OthersToSpawn.Add(other);
    }
}
