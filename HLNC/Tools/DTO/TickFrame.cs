using System;
using System.Collections.Generic;
using HLNC.Utils;
using LiteDB;

namespace HLNC.Editor.DTO
{
    public class TickFrame
    {
        public Guid WorldId { get; set; }
        public int Id { get; set; }
        public int GreatestSize { get; set; }
        public DateTime Timestamp { get; set; }
        public BsonDocument PeerPayloads { get; set; } = new BsonDocument();
        public List<BsonDocument> Logs { get; set; } = new List<BsonDocument>();
        public BsonDocument WorldState { get; set; } = new BsonDocument();
    }
}
