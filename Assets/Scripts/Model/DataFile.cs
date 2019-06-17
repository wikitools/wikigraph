using System.IO;

namespace Model {
	public struct DataFile {
		public FileStream Stream;
		public long Length;
	}

	public enum DataFileType {
		INFO,
		MAP,
		GRAPH,
		TITLES
	}
}