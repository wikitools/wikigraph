using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
	public static void Clamp(ref Vector2 vector, Vector2 min, Vector2 max) {
		vector.x = Mathf.Clamp(vector.x, min.x, max.x);
		vector.y = Mathf.Clamp(vector.y, min.y, max.y);
	}

	public static int Mod(int x, int m) {
		return (x % m + m) % m;
	}

	public static List<T> ScrollList<T>(List<T> list, ref int startIndex, int scrollAmount, int elementNumber) {
		if (scrollAmount != 0) {
			startIndex = Mod(startIndex + scrollAmount, list.Count);
			var absScroll = Mathf.Abs(scrollAmount);
			if(startIndex % absScroll != 0)
				startIndex = (startIndex / absScroll + (scrollAmount > 0 ? 0 : 1)) * absScroll;
		}
		if (elementNumber > list.Count)
			elementNumber = list.Count;
		List<T> subList = new List<T>(list.GetRange(startIndex, Math.Min(elementNumber, list.Count - startIndex)));
		if (subList.Count < elementNumber)
			subList.AddRange(list.GetRange(0, elementNumber - subList.Count));
		return subList;
	}
}