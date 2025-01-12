using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HLNC.Editor.DTO;
using HLNC.Serialization;
using HLNC.Utils;
using LiteDB;

namespace HLNC.Editor
{
    [Tool]
    public partial class WorldDebug : Panel
    {
        [Export]
        public RichTextLabel worldIdLabel;
        private LiteDatabase db;
        private ENetConnection debugConnection;
        private int port;
        private TickFrame incomingTickFrame;
        public int SelectedTickFrameId = -1;
        private int greatestStateSize = 0;
        private Guid worldId;
        private Dictionary<int, TickFrame> tickFrameBuffer = [];
        public bool disconnected = false;

        [Signal]
        public delegate void TickFrameReceivedEventHandler(int id);

        [Signal]
        public delegate void TickFrameUpdatedEventHandler(int id);

        [Signal]
        public delegate void TickFrameSelectedEventHandler(Control tickFrame);

        [Signal]
        public delegate void LogEventHandler(int frameId, string timestamp, string level, string message);

        public void _OnTickFrameSelected(Control tickFrame)
        {
            SelectedTickFrameId = tickFrame.Get("tick_frame_id").AsInt32();
        }

        public void Setup(Guid worldId, int port, LiteDatabase db)
        {
            this.db = db;
            this.worldId = worldId;
            var uuidSegments = worldId.ToString().Split("-");
            Name = uuidSegments[^1] ?? "World";
            worldIdLabel.Text = worldId.ToString();
            debugConnection = new ENetConnection();
            debugConnection.CreateHost();
            debugConnection.Compress(ENetConnection.CompressionMode.RangeCoder);
            debugConnection.ConnectToHost("127.0.0.1", port);
            this.port = port;
        }

        public override void _ExitTree()
        {
            if (incomingTickFrame != null)
            {
                db.GetCollection<TickFrame>("tick_frames").Insert(incomingTickFrame);
                incomingTickFrame = null;
            }
        }

        public override void _Process(double delta)
        {
            if (debugConnection == null || disconnected)
            {
                return;
            }
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
                    case ENetConnection.EventType.Receive:
                        var data = new HLBuffer(packetPeer.GetPacket());
                        var debugDataType = (WorldRunner.DebugDataType)HLBytes.UnpackByte(data);
                        switch (debugDataType)
                        {
                            case WorldRunner.DebugDataType.TICK:
                                {
                                    if (incomingTickFrame != null)
                                    {
                                        db.GetCollection<TickFrame>("tick_frames").Insert(incomingTickFrame);
                                        incomingTickFrame = null;
                                    }
                                    greatestStateSize = 0;
                                    var milliseconds = HLBytes.UnpackInt64(data);
                                    var datetime = new DateTime(milliseconds * TimeSpan.TicksPerMillisecond);
                                    incomingTickFrame = new TickFrame { Id = HLBytes.UnpackInt32(data), Timestamp = datetime, WorldId = worldId };
                                    tickFrameBuffer[incomingTickFrame.Id] = incomingTickFrame;
                                    EmitSignal(SignalName.TickFrameReceived, incomingTickFrame.Id);
                                }
                                break;
                            case WorldRunner.DebugDataType.PAYLOADS:
                                {
                                    if (incomingTickFrame == null)
                                    {
                                        break;
                                    }
                                    var peerId = HLBytes.UnpackVariant(data);
                                    var payload = HLBytes.UnpackByteArray(data, untilEnd: true);
                                    greatestStateSize = Math.Max(greatestStateSize, payload.Length);
                                    incomingTickFrame.PeerPayloads[peerId.ToString()] = payload;
                                    incomingTickFrame.GreatestSize = greatestStateSize;
                                    EmitSignal(SignalName.TickFrameUpdated, incomingTickFrame.Id);
                                }
                                break;
                            case WorldRunner.DebugDataType.LOGS:
                                {
                                    var level = (Debugger.DebugLevel)HLBytes.UnpackByte(data);
                                    var message = HLBytes.UnpackString(data);
                                    incomingTickFrame.Logs.Add(new LiteDB.BsonDocument {
                                        ["level"] = (int)level,
                                        ["message"] = message,
                                    });
                                    EmitSignal(SignalName.Log, incomingTickFrame.Id, incomingTickFrame.Timestamp.ToString(), level.ToString(), message);
                                    EmitSignal(SignalName.TickFrameUpdated, incomingTickFrame.Id);
                                }
                                break;
                            case WorldRunner.DebugDataType.EXPORT:
                                {
                                    var fullGameState = HLBytes.UnpackByteArray(data, untilEnd: true);
                                    incomingTickFrame.WorldState = BsonSerializer.Deserialize(fullGameState);
                                    EmitSignal(SignalName.TickFrameUpdated, incomingTickFrame.Id);
                                }
                                break;
                        }
                        break;
                }
            }
        }

        public Godot.Collections.Dictionary GetFrameData(int id)
        {
            TickFrame tickFrameData;
            if (tickFrameBuffer.ContainsKey(id))
            {
                tickFrameData = tickFrameBuffer[id];
            }
            else
            {
                tickFrameData = db.GetCollection<TickFrame>("tick_frames").FindById(id);
                tickFrameBuffer[id] = tickFrameData;
            }

            var logsList = new Godot.Collections.Array();
            foreach (var log in tickFrameData.Logs)
            {
                var logDict = new Godot.Collections.Dictionary();
                logDict["id"] = tickFrameData.Id;
                logDict["level"] = ((Debugger.DebugLevel)log["level"].AsInt32).ToString();
                logDict["message"] = log["message"].AsString;
                logDict["timestamp"] = log["timestamp"].AsString;
                logsList.Add(logDict);
            }

            var result =  new Godot.Collections.Dictionary {
                ["details"] = new Godot.Collections.Dictionary {
                    ["Tick"] = new Godot.Collections.Dictionary {
                        ["ID"] = tickFrameData.Id,
                        ["Timestamp"] = tickFrameData.Timestamp.ToString(),
                        ["Greatest Size"] = tickFrameData.GreatestSize,
                    },
                },
                ["logs"] = logsList,
                ["world_state"] = Json.ParseString(tickFrameData.WorldState.ToString()).AsGodotDictionary(),
            };

            return result;
        }
    }
}

