using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Services.SyncBuffer {
	public class ConnectionSyncBuffer : DoubleSyncBuffer {
		public ConnectionSyncBuffer(Action<string> syncLoaded, Action<string> syncUnloaded) : base(syncLoaded, syncUnloaded) { }

		public void OnConnectionLoaded(uint from, uint to) {
			loadedBuffer.Sync($"{from} {to}");
		}

		public void OnConnectionUnloaded(uint from, uint to) {
			unloadedBuffer.Sync($"{from} {to}");
		}

		public static List<Tuple<uint, uint>> ParseConnections(string stream) {
			return SyncBuffer.SplitObjectStream(stream).Select(ParseConnection).ToList();
		}

		private static Tuple<uint, uint> ParseConnection(string value) {
			var items = value.Split(' ');
			return new Tuple<uint, uint>(uint.Parse(items[0]), uint.Parse(items[1]));
		}
	}
}