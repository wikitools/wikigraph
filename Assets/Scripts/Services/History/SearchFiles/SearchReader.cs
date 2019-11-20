using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;


namespace Services.SearchFiles {

	public class SearchReader {
		string searchPath;
		int linesNumber;
		long length;
		public static Action<long> onIndexRead;
		FileStream stream;


		public SearchReader(string path) {
			searchPath = path;
			linesNumber = File.ReadLines(searchPath).Count();
			stream = File.OpenRead(path);
			length = new FileInfo(path).Length;
		}



		public string ReadOneLine(long n) {
			stream.Seek(n, SeekOrigin.Begin);
			var sr = new StreamReader(stream, Encoding.UTF8);
			sr.ReadLine();
			return sr.ReadLine();
		}

		public string[] ReadXLinesFromN(int x, long n) {
			stream.Seek(n, SeekOrigin.Begin);
			var sr = new StreamReader(stream, Encoding.UTF8);
			string[] tab = new string[x];
			sr.ReadLine();
			for (int i = 0; i < x; i++) {
				tab[i] = sr.ReadLine();
			}
			return tab;
		}


		public IEnumerator BinSearch(string prefix) {
			long min = 0;
			long max = length - 1;
			long diff = 20;
			StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase;
			long mid = 0;
			while (min <= max) {
				if (mid == (min + max) / 2) break;
				mid = (min + max) / 2;
				string line = ReadOneLine(mid);
				if (line.StartsWith(prefix, stringComparison)) {
					if (max - min < diff || mid == 0 || !ReadOneLine(mid - 1).StartsWith(prefix, stringComparison)) {
						while (mid > 0 && ReadOneLine(mid - 1).StartsWith(prefix, stringComparison)) {
							mid--;
						}
						onIndexRead(mid);
						yield break;

					}


				}
				if (string.Compare(line, prefix, stringComparison) > 0) {
					max = mid;
				}
				else {
					min = mid;
				}
			}
			onIndexRead(mid);
			yield break;
		}
	}

}
