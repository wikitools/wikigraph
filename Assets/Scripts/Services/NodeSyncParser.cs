using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Services {
	public class NodeSyncParser {
		private const int MAX_CHARS_PER_SYNC = 4096;
		private const char NODE_DIVIDER = ';';
		
		private string loadedNodeBuffer = "";
		private string unloadedNodeBuffer = "";

		private readonly Action<string> syncLoadedNodes;
		private readonly Action<string> syncUnloadedNodes;

		public NodeSyncParser(Action<string> syncLoadedNodes, Action<string> syncUnloadedNodes) {
			this.syncLoadedNodes = syncLoadedNodes;
			this.syncUnloadedNodes = syncUnloadedNodes;
		}

		public void OnNodeUnloaded(uint id) {
			HandleNode(ref unloadedNodeBuffer, $"{id}", syncUnloadedNodes);
		}

		public void OnNodeLoaded(uint id, Vector3 position) {
			HandleNode(ref loadedNodeBuffer, $"{new LoadedNodeSync(id, position)}", syncLoadedNodes);
		}

		public void SyncRemainingNodes() {//TODO need to sync node-sync-end event?
			SyncNodes(syncLoadedNodes, ref loadedNodeBuffer);
			SyncNodes(syncUnloadedNodes, ref unloadedNodeBuffer);
		}

		public List<uint> ParseUnloadedNodes(string nodeStream) {
			return nodeStream.Split(NODE_DIVIDER).Select(uint.Parse).ToList();
		}

		public List<LoadedNodeSync> ParseLoadedNodes(string nodeStream) {
			return nodeStream.Split(NODE_DIVIDER).Select(entry => (LoadedNodeSync) entry).ToList();
		}

		private void HandleNode(ref string buffer, string node, Action<string> syncFunction) {
			node += NODE_DIVIDER;
			if (buffer.Length + node.Length > MAX_CHARS_PER_SYNC)
				SyncNodes(syncFunction, ref buffer);
			buffer += node;
		}

		private void SyncNodes(Action<string> syncFunction, ref string buffer) {
			if(buffer.Length == 0)
				return;
			syncFunction(buffer.Remove(buffer.Length - 1));
			buffer = "";
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