using System;
using Godot;
using HLNC.Editor.DTO;
using HLNC.Serialization;
using LiteDB;

namespace HLNC.Editor
{
    [Tool]
    public partial class ServerDebugClient : Window
    {
        private ENetConnection debugConnection = new();
        private PackedScene debugPanelScene = GD.Load<PackedScene>("res://addons/HLNC/Tools/Debugger/world_debug.tscn");
        private LiteDatabase db;

        public void _OnCloseRequested()
        {
            Hide();
        }

        public override void _EnterTree()
        {
            GetTree().MultiplayerPoll = false;
            debugConnection.CreateHost();
            debugConnection.Compress(ENetConnection.CompressionMode.RangeCoder);
            debugConnection.ConnectToHost("127.0.0.1", 59910);
        }

        public override void _ExitTree()
        {
            debugConnection.Destroy();
            if (db != null)
            {
                db.Dispose();
            }
        }

        private void OnDebugConnect()
        {
            Title = "Server Debug Client (Online)";
            db?.Dispose();
            foreach (var child in GetNode("Container/TabContainer").GetChildren())
            {
                child.QueueFree();
            }
            if (!Visible)
            {
                PopupCentered();
                Show();
            }
            try
            {
                string dbFilePath = $"debug_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                db = new LiteDatabase(dbFilePath);
                var tickFrames = db.GetCollection<TickFrame>("tick_frames");
                tickFrames.EnsureIndex(x => x.Id);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error creating database: {e}");
                return;
            }
        }

        public override void _Process(double delta)
        {
            while (true)
            {
                var enetEvent = debugConnection.Service();
                var eventType = enetEvent[0].As<ENetConnection.EventType>();
                if (eventType == ENetConnection.EventType.None || eventType == (ENetConnection.EventType)(-1))
                {
                    break;
                }

                var packetPeer = enetEvent[1].As<ENetPacketPeer>();
                switch (eventType)
                {
                    case ENetConnection.EventType.Connect:
                        packetPeer.SetTimeout(1, 1000, 1000);
                        OnDebugConnect();
                        break;

                    case ENetConnection.EventType.Disconnect:
                        Title = "Server Debug Client (Offline)";
                        foreach (var child in GetNode("Container/TabContainer").GetChildren())
                        {
                            child.Set("disconnected", true);
                        }
                        debugConnection.Destroy();
                        debugConnection = new ENetConnection();
                        debugConnection.CreateHost();
                        debugConnection.Compress(ENetConnection.CompressionMode.RangeCoder);
                        debugConnection.ConnectToHost("127.0.0.1", 59910);
                        return;

                    case ENetConnection.EventType.Receive:
                        var data = packetPeer.GetPacket();
                        var packet = new HLBuffer(data);
                        var worldId = new Guid(HLBytes.UnpackByteArray(packet, 16));
                        var port = HLBytes.UnpackInt32(packet);
                        var debugPanel = debugPanelScene.Instantiate<WorldDebug>();
                        GetNode("Container/TabContainer").AddChild(debugPanel);
                        debugPanel.Setup(worldId, port, db);
                        break;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
