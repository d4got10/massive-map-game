﻿using System.Net.Sockets;

namespace MapServer
{
    partial class Program
    {
        public class Player
        {
            public int X;
            public int Y;
            public int ID;
            public Socket Socket;

            public Player(int x, int y, int id)
            {
                X = x;
                Y = y;
                ID = id;
            }
        }
    }
}
