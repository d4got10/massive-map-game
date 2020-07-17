using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Jobs;

public class Client : IDisposable
{
    public static Action<List<PlayerData>> OnGetOtherCharacters;
    public static Action<int, int, int> OnOtherPlayerMoved;

    private static Socket _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    private static string _ip;
    private static int _port;
    private static PlayerData _self;
    public static List<PlayerData> Others;
    
    private static MemoryStream _memoryStream = new MemoryStream(new byte[128], 0, 128, true, true);
    private static BinaryWriter _writer = new BinaryWriter(_memoryStream);
    private static BinaryReader _reader = new BinaryReader(_memoryStream);

    public int ID => _self.ID;
    public Action<PlayerData> OnDataChanged;

    public class PlayerData
    {
        public int X;
        public int Y;
        public int ID;

        public PlayerData()
        {
        }

        public PlayerData(PlayerData data)
        {
            X = data.X;
            Y = data.Y;
            ID = data.ID;
        }

        public PlayerData(int x, int y, int id)
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

        _self = new PlayerData();

        _self.X = Mathf.RoundToInt(player.transform.position.x);
        _self.Y = Mathf.RoundToInt(player.transform.position.y);
    }

    public void Connect()
    {
        _socket.Connect(_ip, _port);
        SendPacket(PacketInfo.ID, null);
        BeginReceiving();
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
                _writer.Write(_self.ID);

                var move = (Vector2)data;
                _writer.Write(Mathf.RoundToInt(move.x));
                _writer.Write(Mathf.RoundToInt(move.y));
                _socket.Send(_memoryStream.GetBuffer());
                break;
            case PacketInfo.Disconnect:
                _writer.Write(2);
                _writer.Write(_self.ID);
                _socket.Send(_memoryStream.GetBuffer());
                break;
        }       
    }

    public async void BeginReceiving()
    {
        while (_socket.Connected)
        {
            _socket.BeginReceive(_memoryStream.GetBuffer(), 0, 128, SocketFlags.None, ReceivePacket, null);
            await Task.Yield();
        }
    }

    public void ReceivePacket(IAsyncResult ar)
    {
        _memoryStream.Position = 0;
        int count = _socket.EndReceive(ar);

        Debug.Log($"Code:{_reader.ReadInt32()} ID:{_reader.ReadInt32()}  X:{_reader.ReadInt32()}  Y:{_reader.ReadInt32()}");
        _memoryStream.Position = 0;

        int code = _reader.ReadInt32();

        switch (code)
        {
            case 0: 
                _self.ID = _reader.ReadInt32();
                _self.X = _reader.ReadInt32();
                _self.Y = _reader.ReadInt32();

                int playerCount = _reader.ReadInt32();
                Others = new List<PlayerData>();
                for(int i = 0; i < playerCount; i++)
                {
                    var other = new PlayerData();
                    other.ID = _reader.ReadInt32();
                    other.X = _reader.ReadInt32();
                    other.Y = _reader.ReadInt32();
                    Others.Add(other);
                }

                OnGetOtherCharacters?.Invoke(Others);
                OnDataChanged?.Invoke(new PlayerData(_self));
                break;
            case 1:
                int id = _reader.ReadInt32();
                if (_self.ID == id)
                {
                    _self.X = _reader.ReadInt32();
                    _self.Y = _reader.ReadInt32();
                    OnDataChanged?.Invoke(new PlayerData(_self));
                }
                else
                {
                    OnOtherPlayerMoved?.Invoke(id, _reader.ReadInt32(), _reader.ReadInt32());
                }
                break;
        }
    }

    public void Dispose()
    {
        _socket.Disconnect(false);
    }

    public enum PacketInfo
    {
        ID,
        Move,
        Disconnect
    }
}
