using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour, IGridCell
{
    [SerializeField] private GameGrid _gameGrid;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private TextMeshProUGUI _text;

    public int ActionPoints { get; private set; }

    private Vector2 _newPosition = new Vector2();
    
    public string Name;
    public Color Color;
    public bool Setup;

    private bool _connected;

    private Client _client;

    public void Begin(string ip)
    {
        _gameGrid.AddPlayer(this, transform.position);

        _client = new Client(ip, 904, this);
        _client.OnPositionChanged += ChangePosition;
        _client.OnSelfReceived += SetupPlayer;
        _client.Connect();
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt("ID", _client.ID);
        _client.SendPacket(Client.PacketInfo.Disconnect, null);
        _client.OnPositionChanged -= ChangePosition;
        _client.OnSelfReceived -= SetupPlayer;
        _client.Dispose();
    }

    public void ChangePosition(Client.PlayerData data)
    {
        _newPosition = new Vector3(data.X, data.Y);
        _connected = true;
    }

    public void SetupPlayer(Client.PlayerData data)
    {
        Color = new Color(data.Color.Item1 / 256f, data.Color.Item2 / 256f, data.Color.Item3 / 256f);
        Name = data.Name;
        Setup = true;
    }

    private void Update()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.T))
            Debug.Log(_client.ID);
        if (_connected)
        {
            _connected = false;
            transform.position = _newPosition;
        }
        if (Setup)
        {
            _renderer.color = Color;
            _text.text = Name;
            Setup = false;
        }
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
