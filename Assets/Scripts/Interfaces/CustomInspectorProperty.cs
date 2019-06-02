using UnityEditor;

namespace Interfaces {
	public interface CustomInspectorProperty {
		void DrawInInspector(SerializedProperty prop);
	}
}