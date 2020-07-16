using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

namespace MapServer
{
    partial class Program
    {
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Random random = new Random();
        private static List<Player> players = new List<Player>();
        private static Grid grid = new Grid(10);

        static void Main(string[] args)
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, 904));
            socket.Listen(0);

            socket.BeginAccept(new AsyncCallback(AcceptCallback), null);

            Console.ReadLine();
        }

        static void AcceptCallback(IAsyncResult ar)
        {
            Socket client = socket.EndAccept(ar);

            var playerThread = new Thread(HandleClient);
            playerThread.Start(client);

            socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        static void HandleClient(object o)
        {
            Socket client = (Socket)o;
            byte[] buffer = new byte[128];

            MemoryStream ms = new MemoryStream(new byte[128], 0, 128, true, true);
            BinaryReader reader = new BinaryReader(ms);
            BinaryWriter writer = new BinaryWriter(ms);

            while (client.Connected)
            {
                ms.Position = 0;
                client.Receive(ms.GetBuffer());

                int code = reader.ReadInt32();

                switch (code)
                {
                    case 0:
                        int id = players.Count+1;
                        var pos = grid.GetEmptyCell();
                        Player handlePlayer = new Player(pos.Item1, pos.Item2, id);
                        handlePlayer.Socket = client;
                        players.Add(handlePlayer);

                        Console.WriteLine($"Новое подключение от пользователя {id}");
                        Console.WriteLine($"Спавн на точке ({pos.Item1},{pos.Item2})");

                        writer.Write(id);
                        writer.Write(pos.Item1);
                        writer.Write(pos.Item2);
                        client.Send(ms.GetBuffer());
                    break;
                    case 1:
                        id = reader.ReadInt32();
                        Player player = players.Find(t => t.ID == id);
                        if(player != null)
                        {
                            int xMove = reader.ReadInt32();
                            int yMove = reader.ReadInt32();

                            Console.WriteLine($"Получена команда от пользователя {id}");
                            Console.WriteLine($"Движение из точки ({player.X},{player.Y}) в ({player.X + xMove},{player.Y + yMove})");

                            (int, int) position = (player.X + xMove, player.Y + yMove);
                            if (grid.PlayerCanMove(position))
                            {
                                player.X = position.Item1;
                                player.Y = position.Item2;

                                writer.Write(player.X);
                                writer.Write(player.Y);

                                Console.WriteLine($"Комадна выполнена успешно");

                                foreach (var pl in players)
                                {
                                    if(pl.Socket != null)
                                    pl.Socket.Send(ms.GetBuffer());
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Player with ID {id} doesn't exit but tries to move.");
                        }    
                    break;
                    case 2:
                        id = reader.ReadInt32();
                        player = players.Find(t => t.ID == id);
                        player.Socket.Shutdown(SocketShutdown.Both);
                        
                        player.Socket.Disconnect(false);
                        Console.WriteLine($"Пользователь {id} отключился");
                        players.Remove(player);
                    break;
                }
            }
        }
    }
}
