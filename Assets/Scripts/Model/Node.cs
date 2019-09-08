using Controllers;

namespace Model {
	public class Node {
		public uint[] Children;
		public uint[] Parents;

		public readonly uint ID;
		public uint WikiID;

		public string Title;

		public NodeType Type;
		public NodeState State;

		public Node(uint id) {
			ID = id;
		}

		public uint[] GetConnections(ConnectionMode type) => type == ConnectionMode.PARENTS ? Parents : Children;

		public static bool operator ==(Node one, Node two) {
			return ReferenceEquals(one, null) ? ReferenceEquals(two, null) : one.Equals(two);
		}

		public static bool operator !=(Node one, Node two) {
			return !(one == two);
		}

		public bool Equals(Node other) {
			return !ReferenceEquals(other, null) && ID == other.ID;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(obj, null))
				return false;
			return obj is Node && Equals((Node) obj);
		}

		public override int GetHashCode() {
			return ID.GetHashCode();
		}

		public override string ToString() {
			return ID.ToString();
		}
	}

	public enum NodeState {
		SELECTED = 3,
		HIGHLIGHTED = 2,
		ACTIVE = 1,
		DISABLED = 0
	}

	public enum NodeType {
		ARTICLE,
		CATEGORY
	}
}