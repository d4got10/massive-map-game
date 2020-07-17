using System.Collections.Generic;

namespace MapServer
{
    partial class Program
    {
        public class Grid
        {
            public Dictionary<(int, int), CellContentType> Data;
            public int Size;

            public Grid(int size)
            {
                Size = size;
                Size += Size % 2;
                
                Data = new Dictionary<(int, int), CellContentType>();

                for(int x = -size/2; x <= size/2; x++)
                    for (int y = -size / 2; y <= size / 2; y++)
                        Data.Add((x, y), CellContentType.Empty);
            }

            public bool PlayerCanMove((int,int) position)
            {
                if(Data.TryGetValue(position, out var cellContentType))
                {
                    if (cellContentType == CellContentType.Empty)
                        return true;
                }
                return false;
            }

            public void AddPlayer(Player player)
            {
                Data[(player.X, player.Y)] = CellContentType.Player;
            }

            public void RemovePlayer(Player player)
            {
                Data[(player.X, player.Y)] = CellContentType.Empty;
            }

            public void PlayerMoved((int,int) position, (int,int) direction)
            {
                Data[position] = CellContentType.Empty;
                Data[(position.Item1 + direction.Item1, position.Item2 + direction.Item2)] = CellContentType.Player;
            }

            public (int, int) GetEmptyCell()
            {
                int x = 0;
                int y = 0;

                while (!(PlayerCanMove((x, y))))
                {
                    x = random.Next(-Size / 2, Size / 2 + 1);
                    y = random.Next(-Size / 2, Size / 2 + 1);
                }
                return (x, y);
            }
        }
    }
}
