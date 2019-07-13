namespace Model {
	public struct Node {
		public uint[] Children;
		public uint[] Parents;
		
		public uint ID;
		public uint WikiID;
		
		public string Title;
		
		public NodeType Type;
		public NodeState State;

		public static bool operator ==(Node one, Node two) {
			return one.ID == two.ID;
		}

		public static bool operator !=(Node one, Node two) {
			return !(one == two);
		}
		
		public bool Equals(Node other) {
			return ID == other.ID;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is Node && Equals((Node) obj);
		}

		public override int GetHashCode() {
			return (int) ID;
		}
	}

	public enum NodeState {
		ACTIVE, SELECTED, HIGHLIGHTED, DISABLED
	}

	public enum NodeType {
		ARTICLE, CATEGORY
	}
}