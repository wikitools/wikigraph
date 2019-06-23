namespace Model {
	public struct Node {
		public uint[] Children;
		public uint[] Parents;
		
		public uint ID;
		public uint WikiID;
		
		public string Title;
		
		public NodeType Type;
		public NodeState State;
	}

	public enum NodeState {
		LOADED, ACTIVE, DISABLED
	}

	public enum NodeType {
		ARTICLE, CATEGORY
	}
}