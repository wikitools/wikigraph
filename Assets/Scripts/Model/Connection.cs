using System;

namespace Model {
	public class Connection: Tuple<Node, Node> {
		public Route Route;
		public Connection(Node one, Node two) : base(one, two) { }

		public static Tuple<uint, uint> asTuple(Node one, Node two) {
			return new Tuple<uint, uint>(one.ID, two.ID);
		}
		
		public static Tuple<uint, uint> asTuple(Connection connection) {
			return asTuple(connection.Item1, connection.Item2);
		}
		
		public static bool operator ==(Connection one, Connection two) {
			return ReferenceEquals(one, null) ? ReferenceEquals(two, null) : one.Equals(two);
		}

		public static bool operator !=(Connection one, Connection two) {
			return !(one == two);
		}
		
		protected bool Equals(Connection other) {
			if (ReferenceEquals(other, null)) 
				return false;
			return Item1 == other.Item1 && Item2 == other.Item2 || Item1 == other.Item2 && Item2 == other.Item1;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(obj, null)) 
				return false;
			return obj is Connection && Equals((Connection) obj);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			return $"{Item1} {Item2}";
		}
	}
}