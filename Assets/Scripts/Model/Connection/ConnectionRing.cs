using System;
using UnityEngine;

namespace Model.Connection {
	[Serializable]
	public class ConnectionRing {
		public int MaxVisibleConnections;
		public int ChangeConnectionNumber;
		
		public int MaxRowConnections;
		public Vector2 RingAngleSpan;
	}
}