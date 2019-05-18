using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Services {
	public class DataFileReader : IDisposable {
		private readonly Logger<DataFileReader> LOGGER = new Logger<DataFileReader>();

		private Dictionary<DataFileType, DataFile> streams = new Dictionary<DataFileType, DataFile>();
		private readonly string DATA_FILE_PATH = Path.Combine("Assets", "Data");
		private const string DATA_FILE_EXTENSION = "wg";

		public DataFileReader(string dataFilePostfix = "") {
			foreach (DataFileType type in Enum.GetValues(typeof(DataFileType))) {
				loadDataFile(type, dataFilePostfix);
			}
		}

		private void loadDataFile(DataFileType type, string dataFilePostfix) {
			var filePath = Path.Combine(DATA_FILE_PATH + dataFilePostfix, $"{type.ToString().ToLower()}.{DATA_FILE_EXTENSION}");
			using (MemoryMappedFile file = MemoryMappedFile.CreateFromFile(filePath)) {
				streams[type] = new DataFile { Stream = file.CreateViewStream(), Length = new FileInfo(filePath).Length };
			}
		}

		private byte[] readBytes(DataFileType file, int offset, int count) {
			byte[] bytes = new byte[count];
			streams[file].Stream.Position = offset;
			int bytesRead = streams[file].Stream.Read(bytes, 0, count);
			if (bytesRead < count) {
				LOGGER.Warning($"DataFileReader bytes requested: {count}, bytes read: {bytesRead}");
			}
			return bytes;
		}

		public void Dispose() {
			foreach (DataFileType file in streams.Keys) {
				streams[file].Stream.Dispose();
			}
		}

		public struct DataFile {
			public MemoryMappedViewStream Stream;
			public long Length;
		}

		enum DataFileType {
			MAP,
			GRAPH,
			TITLES
		}
	}
}