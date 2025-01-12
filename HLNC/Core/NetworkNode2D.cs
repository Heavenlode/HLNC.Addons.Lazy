using System;
using System.ComponentModel;
using Godot;
using HLNC.Serialization;
using HLNC.Serialization.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace HLNC
{
	/**
		<summary>
		<see cref="Node2D">Node2D</see>, extended with HLNC networking capabilities. This is the most basic networked 3D object.
		On every network tick, all NetworkNode2D nodes in the scene tree automatically have their <see cref="NetworkProperty">network properties</see> updated with the latest data from the server.
		Then, the special <see cref="_NetworkProcess(int)">NetworkProcess</see> method is called, which indicates that a network Tick has occurred.
		Network properties can only update on the server side.
		For a client to update network properties, they must send client inputs to the server via implementing the <see cref="INetworkInputHandler"/> interface, or network function calls via <see cref="NetworkFunction"/> attributes.
		The server receives client inputs, can access them via <see cref="GetInput"/>, and handle them accordingly within <see cref="_NetworkProcess(int)">NetworkProcess</see> to mutate state.
		</summary>
	*/
	public partial class NetworkNode2D : Node2D, INetworkNode, INotifyPropertyChanged, INetworkSerializable<NetworkNode2D>, IBsonSerializable<NetworkNode2D>
	{
		public NetworkController Network { get; internal set; }
		public NetworkNode2D() {
			Network = new NetworkController(this);
		}
		// Cannot have more than 8 serializers
		public IStateSerializer[] Serializers { get; private set; } = [];

		public void SetupSerializers()
		{
			var spawnSerializer = new SpawnSerializer();
			AddChild(spawnSerializer);
			var propertySerializer = new NetworkPropertiesSerializer();
			AddChild(propertySerializer);
			Serializers = [spawnSerializer, propertySerializer];
		}

		public virtual void _WorldReady() {}
		public virtual void _NetworkProcess(int _tick) {}

		/// <inheritdoc/>
		public override void _PhysicsProcess(double delta) {}
		public static HLBuffer NetworkSerialize(WorldRunner currentWorld, NetPeer peer, NetworkNode2D obj)
		{
			var buffer = new HLBuffer();
			if (obj == null)
			{
				HLBytes.Pack(buffer, (byte)0);
				return buffer;
			}
			NetworkId targetNetId;
			byte staticChildId = 0;
			if (obj.Network.IsNetworkScene)
			{
				targetNetId = obj.Network.NetworkId;
			}
			else
			{
				if (NetworkScenesRegister.PackNode(obj.Network.NetworkParent.Node.SceneFilePath, obj.Network.NetworkParent.Node.GetPathTo(obj), out staticChildId))
				{
					targetNetId = obj.Network.NetworkParent.NetworkId;
				}
				else
				{
					throw new Exception($"Failed to pack node: {obj.GetPath()}");
				}
			}
			var peerNodeId = currentWorld.GetPeerWorldState(peer).Value.WorldToPeerNodeMap[targetNetId];
			HLBytes.Pack(buffer, peerNodeId);
			HLBytes.Pack(buffer, staticChildId);
			return buffer;
		}
		public static NetworkNode2D NetworkDeserialize(WorldRunner currentWorld, HLBuffer buffer, NetworkNode2D initialObject)
		{
			var networkID = HLBytes.UnpackByte(buffer);
			if (networkID == 0)
			{
				return null;
			}
			var staticChildId = HLBytes.UnpackByte(buffer);
			var node = currentWorld.GetNodeFromNetworkId(networkID).Node as NetworkNode2D;
			if (staticChildId > 0)
			{
				node = node.GetNodeOrNull(NetworkScenesRegister.UnpackNode(node.SceneFilePath, staticChildId)) as NetworkNode2D;
			}
			return node;
		}

		public BsonValue BsonSerialize(Variant context)
		{
			var doc = new BsonDocument();
			if (Network.IsNetworkScene)
			{
				doc["NetworkId"] = Network.NetworkId;
			}
			else
			{
				doc["NetworkId"] = Network.NetworkParent.NetworkId;
				doc["StaticChildPath"] = Network.NetworkParent.Node.GetPathTo(this).ToString();
			}
			return doc;
		}

		public static NetworkNode2D BsonDeserialize(Variant context, byte[] bson, NetworkNode2D obj)
		{
			var data = BsonSerializer.Deserialize<BsonDocument>(bson);
			if (data.IsBsonNull) return null;
			var doc = data.AsBsonDocument;
			var node = obj == null ? new NetworkNode2D() : obj;
			node.Network._prepareNetworkId = doc["NetworkId"].AsInt64;
			if (doc.Contains("StaticChildPath"))
			{
				node.Network._prepareStaticChildPath = doc["StaticChildPath"].AsString;
			}
			return node;
		}


		public string NodePathFromNetworkScene()
		{
			if (Network.IsNetworkScene)
			{
				return GetPathTo(this);
			}

			return Network.NetworkParent.Node.GetPathTo(this);
		}
	}
}
