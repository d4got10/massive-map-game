using System.Net.Sockets;

namespace MapServer
{
    partial class Program
    {
        public class Player
        {
            public int X;
            public int Y;
            public int ID;
            public string Name;
            public (int, int, int) Color;
            public Socket Socket;

            public Player(int x, int y, int id, string name, (int,int,int) color)
            {
                X = x;
                Y = y;
                ID = id;
                Name = name;
                Color = color;
            }
        }
    }
}
