using Godot;
using HLNC.Addons.Lazy;
using HLNC.Utils;
using HLNC.Utils.Bson;

namespace HLNC
{
	public partial class NetworkController : RefCounted
	{
        public async void LazySpawnReady<T>(byte[] data, NetPeer peer) where T : Node, INetworkNode
		{
			spawnReady[peer] = true;
			preparingSpawn.Remove(peer);
			if (data == null || data.Length == 0)
			{
				return;
			}
			await DataTransformer.FromBSON(context: new LazyPeerStateContext { ContextId = CurrentWorld.GetPeerWorldState(peer).Value.Id }, data, Owner.Node as T);
		}

        // public void PrepareSpawn(NetPeer peer)
		// {
		// 	if (this is not ILazyPeerStatesLoader)
		// 	{
		// 		spawnReady[peer] = true;
		// 		return;
		// 	}
		// 	if (preparingSpawn.ContainsKey(peer) || spawnReady.ContainsKey(peer))
		// 	{
		// 		return;
		// 	}
		// 	preparingSpawn[peer] = true;
		// 	Thread myThread = new Thread(() => _prepareSpawn(peer));
		// 	myThread.Start();
		// }

        
		public async void _prepareSpawnThread(NetPeer peer)
		{
			Debugger.Log($"Loading peer values for: {Owner.Node.Name}", Debugger.DebugLevel.VERBOSE);
			// TODO: Will this cause an exception if peer is altered?
			var peerLoader = this as ILazyPeerStatesLoader;
			var result = await peerLoader.LoadPeerValues(peer);
			CallDeferred("LazySpawnReady", result, peer);
		}
    }
}

