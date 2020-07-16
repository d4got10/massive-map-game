using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Client
{
    private static Socket _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    private static string _ip;
    private static int _port;
    private static Data _data;
    
    private static MemoryStream _memoryStream = new MemoryStream(new byte[128], 0, 128, true, true);
    private static BinaryWriter _writer = new BinaryWriter(_memoryStream);
    private static BinaryReader _reader = new BinaryReader(_memoryStream);

    public int ID => _data.ID;
    public Action<Data> OnDataChanged;

    public class Data
    {
        public int X;
        public int Y;
        public int ID;

        public Data()
        {
        }

        public Data(int x, int y, int id)
        {
            X = x;
            Y = y;
            ID = id;
        }
    }

    public Client(string ip, int port, Player player)
    {
        _ip = ip;
        _port = port;

        _data = new Data();

        _data.X = Mathf.RoundToInt(player.transform.position.x);
        _data.Y = Mathf.RoundToInt(player.transform.position.y);
    }

    public void Connect()
    {
        _socket.Connect(_ip, _port);
        SendPacket(PacketInfo.ID, null);
        ReceivePacket();

        Task.Run(() => { while (true) ReceivePacket(); });
    }

    public void SendPacket(PacketInfo info, object data)
    {
        _memoryStream.Position = 0;
        if(!_socket.Connected)
            _socket.Connect(_ip, _port);

        switch (info)
        {
            case PacketInfo.ID:
                _writer.Write(0);
                _socket.Send(_memoryStream.GetBuffer());
                break;
            case PacketInfo.Move:
                _writer.Write(1);
                _writer.Write(_data.ID);

                var move = (Vector2)data;
                _writer.Write(Mathf.RoundToInt(move.x));
                _writer.Write(Mathf.RoundToInt(move.y));
                _socket.Send(_memoryStream.GetBuffer());
                break;
            case PacketInfo.Disconnect:
                _writer.Write(2);
                _writer.Write(_data.ID);
                _socket.Send(_memoryStream.GetBuffer());
                break;
        }       
    }

    public void ReceivePacket()
    {
        _memoryStream.Position = 0;
        int count = _socket.Receive(_memoryStream.GetBuffer());

        Debug.Log($"Code:{_reader.ReadInt32()} ID:{_reader.ReadInt32()}  X:{_reader.ReadInt32()}  Y:{_reader.ReadInt32()}");
        _memoryStream.Position = 0;

        int code = _reader.ReadInt32();

        switch (code)
        {
            case 0: 
                _data.ID = _reader.ReadInt32();
                _data.X = _reader.ReadInt32();
                _data.Y = _reader.ReadInt32();
                OnDataChanged?.Invoke(_data);
                break;
            case 1: 
                if(_data.ID == _reader.ReadInt32())
                {
                    _data.X = _reader.ReadInt32();
                    _data.Y = _reader.ReadInt32();
                    OnDataChanged?.Invoke(_data);
                }
                break;
        }
    }

    public enum PacketInfo
    {
        ID,
        Move,
        Disconnect
    }
}
