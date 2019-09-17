using UnityEditor;

namespace Inspector {
	public interface CustomInspectorProperty {
#if UNITY_EDITOR
		void DrawInInspector(SerializedProperty property);
#endif
	}
}