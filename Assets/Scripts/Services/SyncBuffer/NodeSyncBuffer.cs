using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Services.SyncBuffer {
	public class NodeSyncBuffer : DoubleSyncBuffer {
		public NodeSyncBuffer(Action<string> syncLoaded, Action<string> syncUnloaded) : base(syncLoaded, syncUnloaded) { }

		public void OnNodeLoaded(uint id, Vector3 position) {
			loadedBuffer.Sync($"{new LoadedNodeSync(id, position)}");
		}

		public void OnNodeUnloaded(uint id) {
			unloadedBuffer.Sync($"{id}");
		}

		public static List<uint> ParseUnloadedNodes(string nodeStream) {
			return SyncBuffer.SplitObjectStream(nodeStream).Select(entry => uint.Parse(entry)).ToList();
		}

		public static List<LoadedNodeSync> ParseLoadedNodes(string nodeStream) {
			return SyncBuffer.SplitObjectStream(nodeStream).Select(entry => (LoadedNodeSync) entry).ToList();
		}

		public struct LoadedNodeSync {
			public readonly uint ID;
			public readonly Vector3 Position;

			public LoadedNodeSync(uint id, Vector3 position) {
				ID = id;
				Position = position;
			}

			public override string ToString() {
				return $"{ID} {Position.x} {Position.y} {Position.z}";
			}

			public static explicit operator LoadedNodeSync(string stream) {
				string[] fields = stream.Split(' ');
				return new LoadedNodeSync(uint.Parse(fields[0]), new Vector3(float.Parse(fields[1]), float.Parse(fields[2]), float.Parse(fields[3])));
			}
		}
	}
}