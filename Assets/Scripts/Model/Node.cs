using System;
using System.Collections.Generic;

namespace Model {
	public struct Node {
		public List<Node> Children;
		public List<Node> Parents;
		
		public uint ID;
		public uint WikiID;
		
		public string Title;
		public NodeType Type;
	}

	public enum NodeType {
		ARTICLE, CATEGORY
	}
}