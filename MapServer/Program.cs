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

        static Player CreateNewPlayer(Socket socket, ref BinaryReader reader)
        {
            var pos = grid.GetEmptyCell();
            int id = players.Count + 1;

            var handlePlayer = new Player(pos.Item1, pos.Item2, id, reader.ReadString(), (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()));
            handlePlayer.Socket = socket;
            players.Add(handlePlayer);
            grid.AddPlayer(handlePlayer);
            return handlePlayer;
        }

        static void HandleClient(object o)
        {
            Socket client = (Socket)o;

            MemoryStream ms = new MemoryStream(new byte[1024], 0, 1024, true, true);
            BinaryReader reader = new BinaryReader(ms);
            BinaryWriter writer = new BinaryWriter(ms);

            while (client.Connected)
            {
                ms.Position = 0;
                try
                {
                    client.Receive(ms.GetBuffer());
                }
                catch
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Disconnect(false);
                    return;
                }
                int code = reader.ReadInt32();

                switch (code)
                {
                    case 0:
                        int id = reader.ReadInt32();

                        Player handlePlayer;

                        bool newPlayer = true;

                        if (id == 0 || id > players.Count)
                        {                          
                            handlePlayer = CreateNewPlayer(client, ref reader);
                            Console.WriteLine($"Новое подключение от нового пользователя {handlePlayer.ID}");
                        }
                        else
                        {
                            handlePlayer = players.Find(t => t.ID == id);
                            if(handlePlayer == null)
                            { 
                                handlePlayer = CreateNewPlayer(client, ref reader);
                                Console.WriteLine($"Новое подключение от нового пользователя {handlePlayer.ID}");
                            }
                            else
                            {
                                Console.WriteLine($"Новое подключение от пользователя {handlePlayer.ID}");
                                handlePlayer.Socket = client;
                                newPlayer = false;
                            }
                        }

                        ms.Position = 4;
                        var pos = (handlePlayer.X, handlePlayer.Y);
                        
                        Console.WriteLine($"Спавн на точке ({pos.Item1},{pos.Item2})");

                        writer.Write(handlePlayer.ID);
                        writer.Write(pos.Item1);
                        writer.Write(pos.Item2);
                        writer.Write(handlePlayer.Name);
                        writer.Write(handlePlayer.Color.Item1);
                        writer.Write(handlePlayer.Color.Item2);
                        writer.Write(handlePlayer.Color.Item3);

                        writer.Write(players.Count - 1);
                        foreach(var other in players)
                        {
                            if(other != handlePlayer)
                            {
                                writer.Write(other.ID);
                                writer.Write(other.X);
                                writer.Write(other.Y);
                                writer.Write(other.Name);
                                writer.Write(other.Color.Item1);
                                writer.Write(other.Color.Item2);
                                writer.Write(other.Color.Item3);
                            }
                        }

                        client.Send(ms.GetBuffer());

                        if (newPlayer)
                        {
                            ms.Position = 0;
                            writer.Write(3);
                            writer.Write(handlePlayer.ID);
                            writer.Write(pos.Item1);
                            writer.Write(pos.Item2);
                            writer.Write(handlePlayer.Name);
                            writer.Write(handlePlayer.Color.Item1);
                            writer.Write(handlePlayer.Color.Item2);
                            writer.Write(handlePlayer.Color.Item3);

                            foreach (var other in players)
                            {
                                if (other != handlePlayer && other.Socket.Connected)
                                {
                                    other.Socket.Send(ms.GetBuffer());
                                }
                            }
                        }
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
                                grid.PlayerMoved((player.X, player.Y), (xMove, yMove));
                                player.X = position.Item1;
                                player.Y = position.Item2;

                                ms.Position = 0;

                                writer.Write(1);
                                writer.Write(player.ID);
                                writer.Write(player.X);
                                writer.Write(player.Y);

                                Console.WriteLine($"Комадна выполнена успешно");

                                foreach (var pl in players)
                                {
                                    if(pl.Socket.Connected)
                                    pl.Socket.Send(ms.GetBuffer());
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Комадна не выполнена");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Player with ID {id} doesn't exit but tries to move.");
                        }    
                    break;
                    case 2:
                        id = reader.ReadInt32();
                        //player = players.Find(t => t.ID == id);
                        client.Shutdown(SocketShutdown.Both);
                        
                        client.Disconnect(false);
                        Console.WriteLine($"Пользователь {id} отключился");
                        //players.Remove(player);
                        //grid.RemovePlayer(player);
                    break;
                    case 3:
                    break;
                }
            }
        }
    }
}
