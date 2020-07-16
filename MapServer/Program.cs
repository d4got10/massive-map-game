using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MapServer
{
    class Program
    {
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main(string[] args)
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, 904));
            socket.Listen(5);
            Socket client = socket.Accept();
            Console.WriteLine("Новое подключение");

            while (true)
            {
                byte[] buffer = new byte[1024];
                client.Receive(buffer);
                Console.WriteLine(String.Concat("Player #1: ",Encoding.ASCII.GetString(buffer)));
            }
        }
    }
}
