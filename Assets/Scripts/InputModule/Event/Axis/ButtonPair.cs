using System;
using InputModule.Event.Button;
using InputModule.Event.Interfaces;
using Inspector;
using UnityEditor;

namespace InputModule.Event.Axis {
	public class ButtonPair: CustomInspectorProperty, InputPoller {
		public bool ReverseAxisDirection;
		public Action<int> OnMove;

		protected int AxisState;
		
		protected void Init(ButtonInput positiveButton, ButtonInput negativeButton) {
			positiveButton.OnPress += () => OnAxisChange(1);
			positiveButton.OnRelease += () => OnAxisChange(0);
			negativeButton.OnPress += () => OnAxisChange(-1);
			negativeButton.OnRelease += () => OnAxisChange(0);
		}

		private void OnAxisChange(int direction) {
			direction *= ReverseAxisDirection ? -1 : 1;
			AxisState = direction;
		}

		public void CheckForInput() {
			if(AxisState != 0) OnMove?.Invoke(AxisState);
		}

		public void DrawInInspector(SerializedProperty property) {
			InspectorUtils.DrawObject(property, () => {
				EditorGUILayout.PropertyField(property.FindPropertyRelative("ReverseAxisDirection"), false);
				InspectorUtils.DrawField(GetType().GetField("PositiveButton"), property.FindPropertyRelative("PositiveButton"), this);
				InspectorUtils.DrawField(GetType().GetField("NegativeButton"), property.FindPropertyRelative("NegativeButton"), this);
			});
		}
	}
}