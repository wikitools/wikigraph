using System.Collections.Generic;

namespace Model {
	public struct Node {
		public List<Node> Children;
		public List<Node> Parents;
		
		public string Title;
		public int WikiId;
		public NodeType Type;
	}

	public enum NodeType {
		ARTICLE, CATEGORY
	}
}