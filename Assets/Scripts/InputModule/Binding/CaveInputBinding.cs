using System;
using System.Collections.Generic;
using InputModule.Event;
using InputModule.Event.Axis;
using InputModule.Event.Button;
using InputModule.Event.Cursor;
using InputModule.Event.Pointer;
using Inspector;
using Services;
using UnityEngine;

namespace InputModule.Binding {
	[Serializable]
	public class CaveInputBinding : InputBinding {
		private static readonly Logger<CaveInputBinding> LOGGER = new Logger<CaveInputBinding>();

		[NotBlocked]
		public FlystickCursor MovementJoystick;
		public FlystickPointer NodePointer;
		public FlystickPointer NodeChooser;
		public FlystickButton ExitNodeTraverseMode;
		public FlystickButtonPairAxis HistoryAxis;
		
		[NotBlocked(InputBlockType.INFO_SPACE)]
		public FlystickButton InfoSpaceToggle;
		[NotBlocked(InputBlockType.CONSOLE)]
		public PCButton OperatorConsoleToggle; // PC type on purpose

		public FlystickButtonAxis ConnectionsScrollAxis;
		

		private static Dictionary<FlystickInstance, int> FlystickBinding = new Dictionary<FlystickInstance, int>();
		public static LzwpInput.Flystick Flystick(FlystickInstance instance) => Lzwp.input.flysticks[FlystickBinding[instance]];
		public LzwpInput.Flystick MainFlystick => Lzwp.input.flysticks[FlystickBinding[FlystickInstance.Primary]];
		public LzwpInput.Flystick SecondaryFlystick => Lzwp.input.flysticks[FlystickBinding[FlystickInstance.Secondary]];

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