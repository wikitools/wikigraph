using System;
using System.Collections.Generic;
using AppInput.Event.Button;
using AppInput.Event.Cursor;
using Services;
using UnityEngine;

namespace AppInput.Binding {
	[Serializable]
	public class CaveInputBinding: InputBinding {
		private static readonly Logger<CaveInputBinding> LOGGER = new Logger<CaveInputBinding>();
		
		public FlystickButton Rot;
		public FlystickCursor Crs;

		public static Dictionary<FlystickInstance, int> FlystickBinding;
		public LzwpInput.Flystick MainFlystick => Lzwp.input.flysticks[FlystickBinding[FlystickInstance.Primary]];
		public LzwpInput.Flystick SecondaryFlystick => Lzwp.input.flysticks[FlystickBinding[FlystickInstance.Secondary]];
		public override void OnRotate(Action<Vector2> callback) {
		}

		public static void SetPrimaryFlystick(int primaryFlystickId) {
			if (primaryFlystickId != 0 && primaryFlystickId != 1) {
				LOGGER.Warning("SetPrimaryFlystick: Invalid flystick index passed");
				return;
			}
			FlystickBinding[FlystickInstance.Primary] = primaryFlystickId;
			FlystickBinding[FlystickInstance.Secondary] = primaryFlystickId == 0 ? 1 : 0;
		}
	}
}