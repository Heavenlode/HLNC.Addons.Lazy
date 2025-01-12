using System.Collections.Generic;
using System.Threading.Tasks;
using HLNC.Serialization;

namespace HLNC.Serialization.Serializers
{
    internal class DespawnSerializer(NetworkNodeWrapper node) : IStateSerializer
    {
        private struct Data
        {

        }

        private NetworkNodeWrapper node = node;

        private Data Deserialize(HLBuffer data)
        {
            return new Data();
        }

        public void Begin() {}

        public void Import(WorldRunner currentWorld, HLBuffer buffer, out NetworkNodeWrapper nodeOut)
        {
            nodeOut = node;
            var data = Deserialize(buffer);
            return;
        }

        public void Cleanup() { }

        public HLBuffer Export(WorldRunner currentWorld, NetPeer peerId)
        {
            var buffer = new HLBuffer();
            // Dictionary<NetPeer, HLBuffer> despawnsBuffer = new Dictionary<NetPeer, HLBuffer>();
            // foreach (int peer_id in NetworkRunner.Instance.MultiplayerInstance.GetPeers())
            // {
            // 	despawnsBuffer[peer_id] = new HLBuffer();
            // }
            // foreach (int peer_id in DespawnBuffers.Keys)
            // {
            // 	if (!despawnsBuffer.ContainsKey(peer_id))
            // 		continue;

            // 	List<int> despawn_ids = new List<int>();
            // 	foreach (int tick_number in DespawnBuffers[peer_id].Keys)
            // 	{
            // 		foreach (int network_id in DespawnBuffers[peer_id][tick_number])
            // 		{
            // 			despawn_ids.Add(network_id);
            // 		}
            // 	}
            // 	HLBytes.Pack(despawnsBuffer[peer_id], despawn_ids.ToArray());
            // }

            // return despawnsBuffer;
            return buffer;
        }

        public void Acknowledge(WorldRunner currentWorld, NetPeer peer, Tick tick)
        {
            // if (!DespawnBuffers.ContainsKey(peer))
            // 	return;
            // if (!DespawnBuffers[peer].ContainsKey(tick))
            // 	return;
            // foreach (int network_id in DespawnBuffers[peer][tick])
            // {
            // 	if (NetworkRunner.Instance.NetworkNodes.ContainsKey(network_id))
            // 	{
            // 		NetworkRunner.Instance.NetworkNodes[network_id].QueueFree();
            // 	}
            // }
            // DespawnBuffers[peer].Remove(tick);
        }
        public void PhysicsProcess(double delta)
        {
        }
    }

}