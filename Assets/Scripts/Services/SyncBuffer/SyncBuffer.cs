using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.SyncBuffer {
	class SyncBuffer {
		private const int MAX_CHARS_PER_SYNC = 4096;
		private const char DIVIDER = ';';
		
		private string buffer = "";

		private readonly Action<string> performSync;

		public SyncBuffer(Action<string> performSync) {
			this.performSync = performSync;
		}

		public void Sync(string obj) {
			obj += DIVIDER;
			if (buffer.Length + obj.Length > MAX_CHARS_PER_SYNC)
				DoSync();
			buffer += obj;
		}

		public void SyncRemaining() {//TODO need to sync node-sync-end event?
			DoSync();
		}

		public static List<string> SplitObjectStream(string nodeStream) {
			return nodeStream.Split(DIVIDER).ToList();
		}

		private void DoSync() {
			if(buffer.Length == 0)
				return;
			performSync(buffer.Remove(buffer.Length - 1));
			buffer = "";
		}
	}
}