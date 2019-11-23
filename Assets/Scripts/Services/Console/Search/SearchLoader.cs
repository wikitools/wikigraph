using System;
using System.Collections;
using System.Collections.Generic;

namespace Services.Search {
	public class SearchLoader {
		int numberOfEntries;
		public SearchReader reader;
		
		public SearchLoader(int number, string path) {
			numberOfEntries = number;
			reader = new SearchReader(path);
		}

		public Dictionary<uint, string> getEntries(long index) {
			IEnumerable<string> lines = reader.ReadXLinesFromN(numberOfEntries, index);
			Dictionary<uint, string> entries = new Dictionary<uint, string>();
			foreach(var line in lines) {
				string[] keyValue = line.Split(';');
				if (keyValue.Length == 2) {
					uint n;
					if(uint.TryParse(keyValue[1], out n)) {
						entries.Add(n, keyValue[0]);
					}
				}
			}
			return entries;
		}
	}
}