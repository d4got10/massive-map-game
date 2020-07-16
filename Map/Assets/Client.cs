using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static class Client
{
    private static Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public static void SendEvent(EventArgs data)
    {
        if(!_socket.Connected)
            _socket.Connect("127.0.0.1", 904);

        switch (data.EventType)
        {
            case EventArgs.EventTypes.Move:
                byte[] buffer = Encoding.ASCII.GetBytes($"moved x: {((Vector2)data.EventData).x}, y: {((Vector2)data.EventData).y}");
                _socket.Send(buffer);
                break;
            default:
                Debug.LogError("Send event error! Wrong Event Type");
                break;
        }       
    }

    public struct EventArgs
    {
        public enum EventTypes
        {
            Move
        }
        public EventTypes EventType;
        public object EventData;
    }
}
