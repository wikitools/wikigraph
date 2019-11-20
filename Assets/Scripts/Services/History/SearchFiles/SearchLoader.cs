using System;
using System.Collections;
using System.Collections.Generic;

namespace Services.SearchFiles {
	public class SearchLoader {

		int numberOfEntries;
		public SearchReader reader;
		

		public SearchLoader(int n, string path) {
			numberOfEntries = n;
			reader = new SearchReader(path);
		}


		public Dictionary<string, uint> getEntries(long index) {
			IEnumerable<string> lines = reader.ReadXLinesFromN(numberOfEntries, index);
			Dictionary<string, uint> entries = new Dictionary<string, uint>();
			foreach(var line in lines) {
				string[] keyValue = line.Split(';');
				if (keyValue.Length == 2) {
					uint n;
					if(uint.TryParse(keyValue[1], out n)) {
						entries.Add(keyValue[0], n);
					}
				}
			}
			return entries;
		}


	}
}