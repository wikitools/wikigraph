using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Model;

namespace Services {
	public class DataFileReader : IDisposable {
		private readonly Logger<DataFileReader> LOGGER = new Logger<DataFileReader>();

		private Dictionary<DataFileType, DataFile> streams = new Dictionary<DataFileType, DataFile>();
		
		private readonly string DATA_FILE_PATH = Path.Combine("Assets", "Data");
		private const string DATA_FILE_EXTENSION = "wg";
		private readonly bool ARE_FILES_LITTLE_ENDIAN = false; // BitConverter.IsLittleEndian;

		public DataFileReader(string dataFilePostfix = "") {
			foreach (DataFileType type in Enum.GetValues(typeof(DataFileType))) {
				loadDataFile(type, dataFilePostfix);
			}
		}

		public ulong ReadLong(DataFileType file, long offset) {
			byte[] bytes = readBytes(file, offset, 4);
			return (ulong) (bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24 | bytes[4] << 32 | bytes[5] << 40 | bytes[6] << 48 | bytes[7] << 56);
		}

		public uint ReadInt(DataFileType file, long offset) {
			byte[] bytes = readBytes(file, offset, 4);
			return (uint) (bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24);
		}

		public uint ReadInt24(DataFileType file, long offset) {
			byte[] bytes = readBytes(file, offset, 4);
			return (uint) (bytes[0] | bytes[1] << 8 | bytes[2] << 16);
		}

		public ushort ReadShort(DataFileType file, long offset) {
			byte[] bytes = readBytes(file, offset, 4);
			return (ushort) (bytes[0] | bytes[1] << 8);
		}

		public string ReadString(DataFileType file, long offset, int length) {
			byte[] bytes = readBytes(file, offset, length, false);
			return Encoding.UTF8.GetString(bytes);
		}

		private void loadDataFile(DataFileType type, string dataFilePostfix) {
			var filePath = Path.Combine(DATA_FILE_PATH + dataFilePostfix, $"{type.ToString().ToLower()}.{DATA_FILE_EXTENSION}");
			var stream = new FileStream(filePath, FileMode.Open, FileSystemRights.Read, FileShare.Read, 4096, FileOptions.RandomAccess);
			streams[type] = new DataFile { Stream = stream, Length = new FileInfo(filePath).Length };
		}

		private byte[] readBytes(DataFileType file, long offset, int count, bool checkEndianness = true) {
			byte[] bytes = new byte[count];
			streams[file].Stream.Position = offset;
			int bytesRead = streams[file].Stream.Read(bytes, 0, count);
			if (bytesRead < count) {
				LOGGER.Warning($"DataFileReader bytes requested: {count}, bytes read: {bytesRead}");
			}
			return !checkEndianness || ARE_FILES_LITTLE_ENDIAN ? bytes : bytes.Reverse().ToArray();
		}

		public void Dispose() {
			foreach (DataFileType file in streams.Keys) {
				streams[file].Stream.Dispose();
			}
		}
	}
}