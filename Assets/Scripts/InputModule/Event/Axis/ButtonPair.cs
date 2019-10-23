using System;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;
using Inspector;
using UnityEditor;

namespace InputModule.Event.Axis {
	public class ButtonPair : CustomInspectorProperty, InputPoller {
		public bool ReverseAxisDirection;
		public Action<int> OnMove;
		public Action<int> OnInputChange;

		protected int AxisState;

		protected void Init(ButtonInput positiveButton, ButtonInput negativeButton) {
			positiveButton.OnPress += () => OnAxisChange(1);
			positiveButton.OnRelease += () => OnAxisChange(0);
			negativeButton.OnPress += () => OnAxisChange(-1);
			negativeButton.OnRelease += () => OnAxisChange(0);
		}

		private void OnAxisChange(int direction) {
			direction *= ReverseAxisDirection ? -1 : 1;
			if (AxisState != direction)
				OnInputChange?.Invoke(direction);
			AxisState = direction;
		}

		public virtual void CheckForInput() {
			if (AxisState != 0) OnMove?.Invoke(AxisState);
		}

#if UNITY_EDITOR
		public void DrawInInspector(SerializedProperty property) {
			InspectorUtils.DrawObject(property, () => {
				EditorGUILayout.PropertyField(property.FindPropertyRelative("ReverseAxisDirection"), false);
				InspectorUtils.DrawField(GetType().GetField("PositiveButton"), property.FindPropertyRelative("PositiveButton"), this);
				InspectorUtils.DrawField(GetType().GetField("NegativeButton"), property.FindPropertyRelative("NegativeButton"), this);
			});
		}
#endif
	}
}