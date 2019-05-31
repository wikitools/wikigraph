using UnityEngine;

public static class Utils {
	public static void clamp (ref Vector3 vector, Vector3 min, Vector3 max) {
		vector.x = Mathf.Clamp (vector.x, min.x, max.x);
		vector.y = Mathf.Clamp (vector.y, min.y, max.y);
		vector.z = Mathf.Clamp (vector.z, min.z, max.z);
	}
}