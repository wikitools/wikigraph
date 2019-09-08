using System;
using UnityEngine;

namespace Services.ObjectPool {
	[Serializable]
	public class GraphPooledObject {
		public GameObject Container;
		public GameObject Prefab;
		public GameObjectPool Pool;
		public int PreloadNumber;

		private const string POOL_OBJECT_NAME = "Pool";
		public GameObject PoolContainer => Container.transform.Find(POOL_OBJECT_NAME).gameObject;
	}
}