using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IGridCell
{
    public int ActionPoints { get; private set; }

    private GameGrid _gameGrid;

    private void Awake()
    {
        _gameGrid = FindObjectOfType<GameGrid>();  
    }

    private void Start()
    {
        _gameGrid.AddPlayer(this, transform.position);
    }

    private void Update()
    {
        Move();
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

        if (_gameGrid.Move(this, transform.position, moveDirection))
        {
            var eventArgs = new Client.EventArgs();
            eventArgs.EventType = Client.EventArgs.EventTypes.Move;
            eventArgs.EventData = moveDirection;

            Client.SendEvent(eventArgs);

            transform.position += (Vector3)moveDirection;
        }
    }
}
