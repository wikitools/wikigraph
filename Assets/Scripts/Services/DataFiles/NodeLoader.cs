using System;
using Model;

namespace Services.DataFiles {
	public class NodeLoader: IDisposable {
		private DataFileReader fileReader;

		private static class MAP {
			public const ushort GRAPH_OFFSET_SIZE = 4;
			public const ushort TITLE_OFFSET_SIZE = 4;
			public const ushort WIKI_ID_OFFSET_SIZE = 4;
			
			public const ushort LINE_SIZE = GRAPH_OFFSET_SIZE + TITLE_OFFSET_SIZE + WIKI_ID_OFFSET_SIZE;
		}

		private static class GRAPH {
			public const ushort PARENT_LINKS_SIZE = 3;
			public const ushort ID_SIZE = 3;
		}

		public NodeLoader(string dataPack, string dataPackDate) {
			fileReader = new DataFileReader(dataPack, dataPackDate);
		}

		public Node LoadNode(uint id) {
			var node = new Node(id);
			node.Type = id < fileReader.ReadInt(DataFileType.INFO, 0) ? NodeType.ARTICLE : NodeType.CATEGORY;
			long nodeMapFilePos = id * MAP.LINE_SIZE;
			loadNodeConnections(ref node, nodeMapFilePos);
			
			uint nodeTitleFilePos = fileReader.ReadInt(DataFileType.MAP, nodeMapFilePos + MAP.GRAPH_OFFSET_SIZE);
			node.WikiID = fileReader.ReadInt(DataFileType.MAP, nodeMapFilePos + MAP.GRAPH_OFFSET_SIZE + MAP.TITLE_OFFSET_SIZE);
			uint nextNodeTitleFilePos = getNextNodePropPos(DataFileType.TITLES, nodeMapFilePos + MAP.LINE_SIZE + MAP.GRAPH_OFFSET_SIZE);
			node.Title = fileReader.ReadString(DataFileType.TITLES, nodeTitleFilePos, (int) (nextNodeTitleFilePos - nodeTitleFilePos));
			node.Title = node.Title.Replace('_', ' ');
			return node;
		}

		public uint GetNodeNumber() {
			return (uint) (fileReader.GetFileLength(DataFileType.MAP) / MAP.LINE_SIZE);
		}

		private void loadNodeConnections(ref Node node, long nodeMapFilePos) {
			uint nodeGraphFilePos = fileReader.ReadInt(DataFileType.MAP, nodeMapFilePos);
			uint nextNodeGraphFilePos = getNextNodePropPos(DataFileType.GRAPH, nodeMapFilePos + MAP.LINE_SIZE);
			
			node.Parents = new uint[fileReader.ReadInt24(DataFileType.GRAPH, nodeGraphFilePos)];
			nodeGraphFilePos += GRAPH.PARENT_LINKS_SIZE;
			for (var i = 0; i < node.Parents.Length; i++) {
				node.Parents[i] = fileReader.ReadInt24(DataFileType.GRAPH, nodeGraphFilePos + i * GRAPH.ID_SIZE);
			}
			node.Children = new uint[(nextNodeGraphFilePos - nodeGraphFilePos) / GRAPH.ID_SIZE - node.Parents.Length];
			for (var i = 0; i < node.Children.Length; i++) {
				node.Children[i] = fileReader.ReadInt24(DataFileType.GRAPH, nodeGraphFilePos + (node.Parents.Length + i) * GRAPH.ID_SIZE);
			}
			// Shuffle Children and parents arrays with constant seed
			UnityEngine.Random.InitState((int)node.ID);
			ShuffleArray(node.Parents);
			ShuffleArray(node.Children);
		}

		private uint getNextNodePropPos(DataFileType file, long offset) {
			return offset <= fileReader.GetFileLength(DataFileType.MAP) - 4
				? fileReader.ReadInt(DataFileType.MAP, offset)
				: (uint) fileReader.GetFileLength(file);
		}

		private void ShuffleArray(uint[] array) {
			int n = array.Length;
			while (n > 1) {
				int k = UnityEngine.Random.Range(0, n--);
				uint temp = array[n];
				array[n] = array[k];
				array[k] = temp;
			}
		}
		
		public void Dispose() {
			fileReader.Dispose();
		}
	}
}