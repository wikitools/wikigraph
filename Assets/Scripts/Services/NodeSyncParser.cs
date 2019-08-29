using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Services {
	public class NodeSyncParser {
		private const int MAX_NODES_PER_SYNC = 1000;
		
		private readonly HashSet<LoadedNodeSync> loadedNodes = new HashSet<LoadedNodeSync>();
		private readonly HashSet<uint> unloadedNodes = new HashSet<uint>();

		private readonly Action<string> syncLoadedNodes;
		private readonly Action<string> syncUnloadedNodes;

		public NodeSyncParser(Action<string> syncLoadedNodes, Action<string> syncUnloadedNodes) {
			this.syncLoadedNodes = syncLoadedNodes;
			this.syncUnloadedNodes = syncUnloadedNodes;
		}

		public void OnNodeUnloaded(uint id) {
			unloadedNodes.Add(id);
			if (unloadedNodes.Count == MAX_NODES_PER_SYNC)
				syncUnloadedNodes(parseSet(unloadedNodes));
		}

		public void OnNodeLoaded(uint id, Vector3 position) {
			loadedNodes.Add(new LoadedNodeSync(id, position));
			if (loadedNodes.Count == MAX_NODES_PER_SYNC)
				syncLoadedNodes(parseSet(loadedNodes));
		}

		public void SyncRemainingNodes() {
			if(loadedNodes.Count > 0)
				syncLoadedNodes(parseSet(loadedNodes));
			if(unloadedNodes.Count > 0)
				syncUnloadedNodes(parseSet(unloadedNodes));
		}

		public List<uint> ParseUnloadedNodes(string nodeStream) {
			return nodeStream.Split(';').Select(uint.Parse).ToList();
		}

		public List<LoadedNodeSync> ParseLoadedNodes(string nodeStream) {
			return nodeStream.Split(';').Select(entry => (LoadedNodeSync) entry).ToList();
		}

		private string parseSet<T>(HashSet<T> set) {
			string parsedNodes = set.Aggregate("", (sequence, id) => sequence + $"{id};");
			set.Clear();
			return parsedNodes.Remove(parsedNodes.Length - 1);
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