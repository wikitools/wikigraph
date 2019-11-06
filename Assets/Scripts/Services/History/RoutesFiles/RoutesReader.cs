using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using UnityEngine;

namespace Services.RoutesFiles {
	public class RoutesReader : IDisposable {
		
		private readonly Logger<RoutesReader> LOGGER = new Logger<RoutesReader>();

		private List<StreamReader> streams = new List<StreamReader>();

		private readonly string DATA_FILE_PATH = Application.streamingAssetsPath + "/Routes/";
		private const string DATA_FILE_EXTENSION = "wgroute";
		private string[] fileNames;

		public RoutesReader(string dataFilePostfix = "") {
			fileNames = Directory.GetFiles(DATA_FILE_PATH).Where(name => !name.EndsWith(".meta")).ToArray();

			foreach (string name in fileNames) {
				loadDataFile(name, dataFilePostfix);
			}
		}

		public string readLine(int fileIndex) {
			return streams[fileIndex].ReadLine();
		}

		public bool isNotEOF(int fileIndex) {
			if(streams[fileIndex].Peek() < 0) {
				streams[fileIndex].DiscardBufferedData();
				streams[fileIndex].BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
				return false;
			}
			return true;
		}

		public int numberOfRoutes() {
			return streams.Count;
		}

		public string[] namesOfRoutes() {
			return fileNames;
		}

		private void loadDataFile(string name, string dataFilePostfix) {
			var filePath = Path.Combine(DATA_FILE_PATH, $"{name.ToString().ToLower() + dataFilePostfix}");
			streams.Add(new StreamReader(filePath));
		}

		

		public void Dispose() {
			foreach (StreamReader file in streams) {
				file.Dispose();
			}
		}
	}
}

