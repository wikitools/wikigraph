using System;
using InputModule.Event;
using InputModule.Event.Button;
using InputModule.Event.Cursor;
using InputModule.Event.Pointer;
using Inspector;

namespace InputModule.Binding {
	[Serializable]
	public class PcInputBinding : InputBinding {
		[NotBlocked(InputBlockType.INFO_SPACE)]
		public PCCursor RotationInput;

		public PCButtonAxis MainMovementAxis;
		public PCButtonAxis CrossMovementAxis;

		public PCPointer NodePointer;
		public PCPointer NodeChooser;
		public PCButton ExitNodeTraverseMode;
		
		public PCButton ConnectionModeToggle;
		public PCButtonAxis ConnectionScroll;

		public PCButtonAxis HistoryAxis;
		
		[NotBlocked(InputBlockType.INFO_SPACE)]
		public PCButton InfoSpaceToggle;
		[NotBlocked(InputBlockType.CONSOLE)]
		public PCButton OperatorConsoleToggle;
	}
}