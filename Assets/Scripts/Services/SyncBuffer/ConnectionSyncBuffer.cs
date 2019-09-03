using System;

namespace Services.SyncBuffer {
	public class ConnectionSyncBuffer: TwoWaySyncBuffer {
		public ConnectionSyncBuffer(Action<string> syncLoaded, Action<string> syncUnloaded) : base(syncLoaded, syncUnloaded) { }

		public void OnConnectionLoaded(uint id) {
			loadedBuffer.Sync($"{id}");
		}
		
		public void OnConnectionUnloaded(uint id) {
			unloadedBuffer.Sync($"{id}");
		}
	}
}