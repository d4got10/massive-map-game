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
