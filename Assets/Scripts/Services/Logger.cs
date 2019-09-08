using System;

namespace Services {
	public class Logger<T> {
		public void Log(string message) {
			Lzwp.debug?.Log($"[{typeof(T).Name}] {message}");
		}

		public void Warning(string message) {
			Lzwp.debug?.Warning($"[{typeof(T).Name}] {message}");
		}

		public void Error(string message) {
			Lzwp.debug?.Error($"[{typeof(T).Name}] {message}");
		}

		public void Exception(Exception exception) {
			Lzwp.debug?.Exception(exception);
		}
	}
}