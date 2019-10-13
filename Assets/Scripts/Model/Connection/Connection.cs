using System;
using System.Collections.Generic;

namespace Model.Connection {
	public class Connection : Tuple<Node, Node> {
		public Route Route;
		public Connection(Node one, Node two) : base(one.ID <= two.ID ? one : two, one.ID <= two.ID ? two : one) { }

		public List<Node> Ends => new List<Node> {Item1, Item2};

		public static Tuple<uint, uint> AsTuple(Node one, Node two) {
			return new Tuple<uint, uint>(one.ID, two.ID);
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