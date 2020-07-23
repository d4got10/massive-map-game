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

            PopulateServer(100);
            socket.BeginAccept(new AsyncCallback(AcceptCallback), null);

            string command = "";
            while (command != "exit")
            {
                var message = Console.ReadLine();
                if(message[0] == '/')
                {
                    command = message.Substring(1, message.Length-1);
                    ExecuteCommand(command);
                }
                if (command == "manual")
                {
                    command = "";
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    while(key.Key != ConsoleKey.Q)
                    {
                        key = Console.ReadKey(true);
                        switch (key.Key.ToString().ToLower()[0])
                        {
                            case 'w': ExecuteCommand($"move u 1"); break;
                            case 'd': ExecuteCommand($"move r 1"); break;
                            case 's': ExecuteCommand($"move d 1"); break;
                            case 'a': ExecuteCommand($"move l 1"); break;
                        }
                        
                    }
                }
            }
        }

        static void ExecuteCommand(string command)
        {
            if(command.Substring(0, 4) == "move")
            {
                try
                {
                    int xMove = 0;
                    int yMove = 0;
                    char where = command[5];
                    switch (where)
                    {
                        case 'r': xMove = 1; break;
                        case 'l': xMove = -1; break;
                        case 'u': yMove = 1; break;
                        case 'd': yMove = -1; break;
                    }
                    int amount = Convert.ToInt32(char.GetNumericValue(command[7]));

                    xMove *= amount;
                    yMove *= amount;

                    MemoryStream ms = new MemoryStream(new byte[1024], 0, 1024, true, true);
                    BinaryWriter writer = new BinaryWriter(ms);

                    foreach (var player in players)
                    {
                        if(player.Socket == null || !player.Socket.Connected)
                        {
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

                                foreach (var pl in players)
                                {
                                    if (pl.Socket != null && pl.Socket.Connected)
                                        pl.Socket.Send(ms.GetBuffer());
                                }
                            }
                        }
                        Thread.Sleep(50);
                    }
                    Console.WriteLine("Command executed");
                }
                catch
                {
                }
            }
        }

        static void PopulateServer(int num)
        {
            var id = 0;
            for (int i = 0; i < num; i++)
            {
                id++;
                var pos = grid.GetEmptyCell();
                var player = new Player(pos.Item1, pos.Item2, id, random.Next(100000, 1000000).ToString(), (random.Next(0, 256), random.Next(0, 256), random.Next(0, 256)));

                players.Add(player);
                grid.AddPlayer(player);
            }
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
                    if (!client.Connected)
                    {
                        ms.Position = 0;
                        var disPlayer = players.Find(t => t.Socket == client);
                        if (disPlayer != null)
                        {
                            Console.WriteLine($"Пользователь {disPlayer.ID} потерял соединение");
                        }
                        else
                        {
                            Console.WriteLine($"Пользователь UNKWN потерял соединение");
                        }
                    }
                }
                catch(SocketException e)
                {
                    Console.WriteLine(e.Message);
                    client.Shutdown(SocketShutdown.Both);
                    client.Disconnect(false);
                    return;
                }
                int code = reader.ReadInt32();

                switch (code)
                {
                    case 0:
                        int id = reader.ReadInt32();
                        
                        string name = reader.ReadString();
                        ms.Position = 8;

                        Player handlePlayer;

                        bool newPlayer = true;

                        if (id == 0 || id > players.Count || players.Find(t => t.Name == name) == null)
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

                        var pos = (handlePlayer.X, handlePlayer.Y);

                        Console.WriteLine($"Спавн на точке ({pos.Item1},{pos.Item2})");
                        foreach (var other in players)
                        {
                            if(other != handlePlayer)
                            {
                                ms.Position = 0;
                                writer.Write(3);
                                writer.Write(other.ID);
                                writer.Write(other.X);
                                writer.Write(other.Y);
                                writer.Write(other.Name);
                                writer.Write(other.Color.Item1);
                                writer.Write(other.Color.Item2);
                                writer.Write(other.Color.Item3);
                                client.Send(ms.GetBuffer());
                                Thread.Sleep(50);
                            }
                        }     

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
                                if (other != handlePlayer && other.Socket != null && other.Socket.Connected)
                                {
                                    other.Socket.Send(ms.GetBuffer());
                                }
                            }
                        }

                        ms.Position = 0;

                        writer.Write(0);
                        writer.Write(handlePlayer.ID);
                        writer.Write(handlePlayer.X);
                        writer.Write(handlePlayer.Y);
                        writer.Write(handlePlayer.Name);
                        writer.Write(handlePlayer.Color.Item1);
                        writer.Write(handlePlayer.Color.Item2);
                        writer.Write(handlePlayer.Color.Item3);
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
                                    if(pl.Socket != null && pl.Socket.Connected)
                                    pl.Socket.Send(ms.GetBuffer());
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Комадна не выполнена");
                                if(players.Find(t => (t.X, t.Y) == position) != null)
                                    Console.WriteLine($"{players.Find(t => (t.X,t.Y) == position).Name}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Player with ID {id} doesn't exit but tries to move.");
                            ms.Position = 0;
                            writer.Write(4);
                            client.Send(ms.GetBuffer());
                            //client.Shutdown(SocketShutdown.Both);
                            //client.Disconnect(false);
                        }    
                    break;
                    case 2:
                        id = reader.ReadInt32();
                        //player = players.Find(t => t.ID == id);
                        ms.Position = 0;
                        writer.Write(4);
                        client.Send(ms.GetBuffer());
                        client.Shutdown(SocketShutdown.Both);
                        client.Disconnect(false);
                        Console.WriteLine($"Пользователь {id} отключился");
                        //players.Remove(player);
                        //grid.RemovePlayer(player);
                    break;
                    case 3:
                    break;
                    case 4:
                    break;
                }
            }
        }
    }
}
