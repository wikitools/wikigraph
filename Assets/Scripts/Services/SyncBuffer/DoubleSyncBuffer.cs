using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.SyncBuffer {
	public abstract class DoubleSyncBuffer {
		protected readonly SyncBuffer loadedBuffer;
		protected readonly SyncBuffer unloadedBuffer;

		protected DoubleSyncBuffer(Action<string> syncLoaded, Action<string> syncUnloaded) {
			loadedBuffer = new SyncBuffer(syncLoaded);
			unloadedBuffer = new SyncBuffer(syncUnloaded);
		}

		public void SyncRemaining() { //TODO need to sync node-sync-end event?
			loadedBuffer.SyncRemaining();
			unloadedBuffer.SyncRemaining();
		}

		public static List<uint> ParseNodeIDs(string stream) {
			return SyncBuffer.SplitObjectStream(stream).Select(uint.Parse).ToList();
		}
	}
}