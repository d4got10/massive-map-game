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
    public static Action OnDisconnect;

    private static Socket _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    private static string _ip;
    private static int _port;
    private static PlayerData _self;
    public static List<PlayerData> Others;
    
    private static MemoryStream _memoryStream = new MemoryStream(new byte[1024], 0, 1024, true, true);
    private static BinaryWriter _writer = new BinaryWriter(_memoryStream);
    private static BinaryReader _reader = new BinaryReader(_memoryStream);

    public int ID => _self.ID;
    public Action<PlayerData> OnPositionChanged;
    public Action<PlayerData> OnSelfReceived;

    public class PlayerData
    {
        public int X;
        public int Y;
        public int ID;
        public string Name;
        public (int,int,int) Color;

        public PlayerData()
        {
        }

        public PlayerData(int x, int y, string name, (int,int,int) color, int id)
        {
            X = x;
            Y = y;
            Name = name;
            Color = color;
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

        if (PlayerPrefs.HasKey("ID"))
            _self.ID = PlayerPrefs.GetInt("ID");
        else
            _self.ID = 0;

        _self.Name = player.Name;
        _self.Color = (Mathf.RoundToInt(player.Color.r * 256), Mathf.RoundToInt(player.Color.g * 256), Mathf.RoundToInt(player.Color.b * 256));
    }

    public void Connect()
    {
        _socket.Connect(_ip, _port);
        SendPacket(PacketInfo.ID, _self);
        BeginReceiving();
    }

    public void SendPacket(PacketInfo info, object data)
    {
        _memoryStream.Position = 0;
        if (!_socket.Connected)
        {
            Connect();
        }

        switch (info)
        {
            case PacketInfo.ID:
                _writer.Write(0);
                _writer.Write(_self.ID);
                _writer.Write(_self.Name);
                _writer.Write(_self.Color.Item1);
                _writer.Write(_self.Color.Item2);
                _writer.Write(_self.Color.Item3);      

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
            _socket.BeginReceive(_memoryStream.GetBuffer(), 0, 1024, SocketFlags.None, ReceivePacket, null);
            await Task.Yield();
        }
    }

    public void ReceivePacket(IAsyncResult ar)
    {
        _memoryStream.Position = 0;
        try
        {
            _socket.EndReceive(ar);
        }
        catch(SocketException e)
        {
            Debug.LogError(e.Message);
            Connect();
        }
        Debug.Log($"Code:{_reader.ReadInt32()} ID:{_reader.ReadInt32()}  X:{_reader.ReadInt32()}  Y:{_reader.ReadInt32()}");
        _memoryStream.Position = 0;

        int code = _reader.ReadInt32();

        switch (code)
        {
            case 0:
                _self.ID = _reader.ReadInt32();      
                ReceiveSelf();
                ReceiveOtherPlayers();
                break;
            case 1:
                int id = _reader.ReadInt32();
                if (_self.ID == id)
                    ReceivePosition();
                else
                    OnOtherPlayerMoved?.Invoke(id, _reader.ReadInt32(), _reader.ReadInt32());
                break;
            case 2:
                break;
            case 3:
                OtherPlayerConnected();
                break;
            case 4:
                Disconnect();
                break;
        }
    }

    private void Disconnect()
    {
        OnDisconnect?.Invoke();
    }

    private void ReceiveSelf()
    {
        _self.X = _reader.ReadInt32();
        _self.Y = _reader.ReadInt32();

        _self.Name = _reader.ReadString();

        _self.Color.Item1 = _reader.ReadInt32();
        _self.Color.Item2 = _reader.ReadInt32();
        _self.Color.Item3 = _reader.ReadInt32();

        OnPositionChanged?.Invoke(_self);
        OnSelfReceived?.Invoke(_self);
    }

    private void ReceivePosition()
    {
        _self.X = _reader.ReadInt32();
        _self.Y = _reader.ReadInt32();
        OnPositionChanged?.Invoke(_self);
    }

    private void ReceiveOtherPlayers()
    {
        int playerCount = _reader.ReadInt32();
        Others = new List<PlayerData>();
        for (int i = 0; i < playerCount; i++)
        {
            var other = new PlayerData();
            other.ID = _reader.ReadInt32();
            other.X = _reader.ReadInt32();
            other.Y = _reader.ReadInt32();

            other.Name = _reader.ReadString();

            other.Color.Item1 = _reader.ReadInt32();
            other.Color.Item2 = _reader.ReadInt32();
            other.Color.Item3 = _reader.ReadInt32();
            Others.Add(other);
        }

        OnGetOtherCharacters?.Invoke(Others);
    }

    private void OtherPlayerConnected()
    {
        var others = new List<PlayerData>();
        var otherPlayer = new PlayerData();
        otherPlayer.ID = _reader.ReadInt32();
        otherPlayer.X = _reader.ReadInt32();
        otherPlayer.Y = _reader.ReadInt32();
        otherPlayer.Name = _reader.ReadString();
        otherPlayer.Color.Item1 = _reader.ReadInt32();
        otherPlayer.Color.Item2 = _reader.ReadInt32();
        otherPlayer.Color.Item3 = _reader.ReadInt32();
        others.Add(otherPlayer);
        OnGetOtherCharacters?.Invoke(others);
    }

    public void Dispose()
    {
        _socket.Disconnect(false);
    }

    public enum PacketInfo
    {
        ID,
        Move,
        Disconnect,
        OtherPlayerConnect
    }
}
