using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using UnityEngine;

namespace Services.Routes {
	public class RouteReader : IDisposable {
		private List<StreamReader> streams = new List<StreamReader>();

		private const string DATA_FILE_EXTENSION = ".wgr";
		private const string POSTFIX = "";
		private string routesPath;
		private string[] fileNames;
		private int[] routeLength;

		public RouteReader(string path) {
			routesPath = path;
			if(!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
			fileNames = Directory.GetFiles(routesPath).Where(name => name.EndsWith(DATA_FILE_EXTENSION)).ToArray();

			foreach (string name in fileNames) {
				loadDataFile(name, POSTFIX);
			}
			routeLength = new int[numberOfRoutes()];
			for (int i=0; i<numberOfRoutes();i++) {
				int lines = 0;
				while (isNotEOF(i)) {
					lines++;
					readLine(i);
				}
				routeLength[i] = lines;
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

		public int[] lengthOfRoutes() {
			return routeLength;
		}

		private void loadDataFile(string name, string dataFilePostfix) {
			var filePath = Path.Combine(routesPath, $"{name + dataFilePostfix}");
			streams.Add(new StreamReader(filePath));
		}

		

		public void Dispose() {
			foreach (StreamReader file in streams) {
				file.Dispose();
			}
		}
	}
}

