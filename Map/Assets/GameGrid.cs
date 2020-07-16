using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public Dictionary<Vector2, IGridCell> Grid;

    private void Awake()
    {
        Grid = new Dictionary<Vector2, IGridCell>();
        for(int x = -5; x <= 5; x++)
        {
            for(int y = -5; y <= 5; y++)
            {
                Grid.Add(new Vector2(x, y), null);
            }
        }
    }

    public bool AddPlayer(IGridCell caller, Vector2 position)
    {
        if (Grid.ContainsKey(position) && Grid[position] == null)
        {
            Grid[position] = caller;
            return true;
        }
        return false;
    }

    public bool Move(IGridCell caller, Vector2 position, Vector2 direction)
    {
        if(Grid.ContainsKey(position + direction) && Grid[position + direction] == null)
        {
            Grid[position + direction] = caller;
            Grid[position] = null;
            return true;
        }
        return false;
    }
}

public interface IGridCell
{
}