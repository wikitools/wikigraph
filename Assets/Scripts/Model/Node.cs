using Controllers;

namespace Model {
	public class Node {
		public uint[] Children;
		public uint[] Parents;

		public uint ID;
		public uint WikiID;

		public string Title;

		public NodeType Type;
		public NodeState State;

		public uint[] GetConnections(ConnectionMode type) => type == ConnectionMode.PARENTS ? Parents : Children;

		public static bool operator ==(Node one, Node two) {
			return ReferenceEquals(one, null) ? ReferenceEquals(two, null) : one.Equals(two);
		}

		public static bool operator !=(Node one, Node two) {
			return !(one == two);
		}

		public bool Equals(Node other) {
			if (ReferenceEquals(other, null)) return false;
			return ID == other.ID;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(obj, null)) return false;
			return obj is Node && Equals((Node)obj);
		}

		public override int GetHashCode() {
			return (int)ID;
		}
	}

	public enum NodeState {
		ACTIVE, SELECTED, HIGHLIGHTED, DISABLED
	}

	public enum NodeType {
		ARTICLE, CATEGORY
	}
}