using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using Model;
using UnityEngine;

namespace Services.DataFiles {
	public class DataFileReader : IDisposable {
		private readonly string dataPack;
		private readonly string dataPackDate;
		private readonly Logger<DataFileReader> logger = new Logger<DataFileReader>();
		private Dictionary<DataFileType, DataFile> streams = new Dictionary<DataFileType, DataFile>();
		public static readonly string DATA_FILE_PATH = Path.Combine(Application.streamingAssetsPath, "DataFiles");
		private const string DATA_FILE_EXTENSION = "wg";

		public DataFileReader(string dataPack, string dataPackDate) {
			this.dataPack = dataPack;
			this.dataPackDate = dataPackDate;
			Dispose();
			LoadDataPack();
		}

		private void LoadDataPack() {
			foreach (DataFileType type in Enum.GetValues(typeof(DataFileType)))
				loadDataFile(type);
		}

		public ulong ReadLong(DataFileType file, long offset) {
			byte[] bytes = ReadBytes(file, offset, 8);
			return (ulong) (bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24 | bytes[4] << 32 | bytes[5] << 40 | bytes[6] << 48 | bytes[7] << 56);
		}

		public uint ReadInt(DataFileType file, long offset) {
			byte[] bytes = ReadBytes(file, offset, 4);
			return (uint) (bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24);
		}

		public uint ReadInt24(DataFileType file, long offset) {
			byte[] bytes = ReadBytes(file, offset, 3);
			return (uint) (bytes[0] | bytes[1] << 8 | bytes[2] << 16);
		}

		public uint[] ReadInt24Array(DataFileType file, long offset, int size) {
			byte[] bytes = ReadBytes(file, offset, size * 3);
			uint[] array = new uint[size];
			for (var i = 0; i < size; i++)
				array[i] = (uint) (bytes[i * 3] | bytes[i * 3 + 1] << 8 | bytes[i * 3 + 2] << 16);
			return array;
		}

		public ushort ReadShort(DataFileType file, long offset) {
			byte[] bytes = ReadBytes(file, offset, 2);
			return (ushort) (bytes[0] | bytes[1] << 8);
		}

		public byte ReadByte(DataFileType file, long offset) {
			byte[] bytes = ReadBytes(file, offset, 1);
			return bytes[0];
		}

		public string ReadString(DataFileType file, long offset, int length) {
			byte[] bytes = ReadBytes(file, offset, length);
			return Encoding.UTF8.GetString(bytes);
		}

		public long GetFileLength(DataFileType file) {
			return streams[file].Length;
		}

		private void loadDataFile(DataFileType type) {
			var filePath = GetDataPackFile() + type.ToString().ToLower()[0];
			FileStream stream = null;
			try {
				stream = new FileStream(filePath, FileMode.Open, FileSystemRights.Read, FileShare.Read, 4096, FileOptions.RandomAccess);
			} catch (Exception e) {
				logger.Exception($"Could not load data file {filePath}", e);
			}
			streams[type] = new DataFile {Stream = stream, Length = new FileInfo(filePath).Length};
		}
		
		public string GetDataPackFile() => Path.Combine(GetDataPackDirectory(), $"{dataPack}.{DATA_FILE_EXTENSION}");

		public string GetDataPackDirectory() => Path.Combine(DATA_FILE_PATH, dataPack, dataPackDate);

		private byte[] ReadBytes(DataFileType file, long offset, int count) {
			byte[] bytes = new byte[count];
			streams[file].Stream.Position = offset;
			int bytesRead = streams[file].Stream.Read(bytes, 0, count);
			if (bytesRead < count) {
				logger.Warning($"DataFileReader bytes requested: {count}, bytes read: {bytesRead}");
			}
			return bytes;
		}

		public void Dispose() {
			foreach (DataFileType file in streams.Keys) {
				streams[file].Stream.Dispose();
			}
		}
	}
}