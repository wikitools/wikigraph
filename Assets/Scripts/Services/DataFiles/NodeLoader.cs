using System;
using Model;

namespace Services {
	public class NodeLoader: IDisposable {
		private DataFileReader fileReader;

		public const ushort NODE_TYPE_DIVIDER = 2;
		private static class MAP {
			public const ushort GRAPH_OFFSET_SIZE = 4;
			public const ushort TITLE_OFFSET_SIZE = 4;
			public const ushort WIKI_ID_OFFSET_SIZE = 4;
			
			public const ushort LINE_SIZE = GRAPH_OFFSET_SIZE + TITLE_OFFSET_SIZE + WIKI_ID_OFFSET_SIZE;
		}

		private static class GRAPH {
			public const ushort PARENT_LINKS_SIZE = 1;
			public const ushort ID_SIZE = 3;
		}

		public NodeLoader() {
			fileReader = new DataFileReader();
			Node one = LoadNode(2);
		}

		public Node LoadNode(uint id) {
			var node = new Node();
			node.ID = id;
			node.Type = id < NODE_TYPE_DIVIDER ? NodeType.CATEGORY : NodeType.ARTICLE;
			long mapPos = id * MAP.LINE_SIZE;
			uint graphPos = fileReader.ReadInt(DataFileType.MAP, mapPos);
			uint titlePos = fileReader.ReadInt(DataFileType.MAP, mapPos + MAP.GRAPH_OFFSET_SIZE);
			node.WikiID = fileReader.ReadInt(DataFileType.MAP, mapPos + MAP.GRAPH_OFFSET_SIZE + MAP.TITLE_OFFSET_SIZE);
			uint nextTitlePos = fileReader.ReadInt(DataFileType.MAP, mapPos + MAP.LINE_SIZE + MAP.GRAPH_OFFSET_SIZE);
			node.Title = fileReader.ReadString(DataFileType.TITLES, titlePos, (int) (nextTitlePos - titlePos));
			return node;
		}
		
		public void Dispose() {
			fileReader.Dispose();
		}
	}
}