using System;
using System.Collections.Generic;
using UnityEngine;

namespace Services {
	public class GameObjectPool {
		private readonly Logger<GameObjectPool> LOGGER = new Logger<GameObjectPool>();

		private GameObject prefab;
		private const int LOAD_NUMBER = 100;
		private Stack<GameObject> pool;
		
		public GameObjectPool(GameObject prefab, int preloadNumber) {
			this.prefab = prefab;
			pool = new Stack<GameObject>(preloadNumber);
			preloadElements(preloadNumber);
		}

		private void preloadElements(int number) {
			for (int i = 0; i < Math.Abs(number); i++) {
				pool.Push(GameObject.Instantiate(prefab));
			}
		}

		public GameObject Spawn() {
			if (pool.Count == 0) {
				preloadElements(LOAD_NUMBER);
			}

			var element = pool.Pop();
			if (element == null) {
				return Spawn();
			}
			element.SetActive(true);
			return element;
		}

		public void Despawn(GameObject element) {
			element.SetActive(false);
			pool.Push(element);
		}
	}
}