using UnityEditor;

namespace Inspector {
	public interface CustomInspectorProperty {
		void DrawInInspector(SerializedProperty property);
	}
}