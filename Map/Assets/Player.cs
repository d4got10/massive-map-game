using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IGridCell
{
    public int ActionPoints { get; private set; }

    private Vector2 newPosition;

    private GameGrid _gameGrid;
    private Client _client;

    private void Awake()
    {
        _gameGrid = FindObjectOfType<GameGrid>();  
    }

    private void Start()
    {
        _gameGrid.AddPlayer(this, transform.position);

        _client = new Client("127.0.0.1", 904, this);
        _client.OnDataChanged += ChangePosition;
        _client.Connect();
    }

    private void OnDestroy()
    {
        _client.SendPacket(Client.PacketInfo.Disconnect, null);
        _client.OnDataChanged -= ChangePosition;
        _client.Dispose();
    }

    public void ChangePosition(Client.PlayerData data)
    {
        newPosition = new Vector3(data.X, data.Y);
    }

    private void Update()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.T))
            Debug.Log(_client.ID);
        transform.position = newPosition;
    }

    private void Move()
    {
        var moveDirection = new Vector2();
        if (Input.GetKeyDown(KeyCode.W))
        {
            moveDirection = new Vector2(0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moveDirection = new Vector2(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveDirection = new Vector2(0, -1);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveDirection = new Vector2(1, 0);
        }

        if (moveDirection == Vector2.zero) return;

        if (true || _gameGrid.Move(this, transform.position, moveDirection))
        {

            _client.SendPacket(Client.PacketInfo.Move, moveDirection);
            //transform.position += (Vector3)moveDirection;
        }
    }
}
