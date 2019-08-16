using UnityEngine;

public static class Utils {
	public static void Clamp (ref Vector2 vector, Vector2 min, Vector2 max) {
		vector.x = Mathf.Clamp (vector.x, min.x, max.x);
		vector.y = Mathf.Clamp (vector.y, min.y, max.y);
	}
}