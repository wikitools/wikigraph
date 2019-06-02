using AppInput.Event.Button;
using AppInput.Event.Cursor;

namespace AppInput.Mapping {
	[System.Serializable]
	public class CaveInputMapping: InputMapping {
		public FlystickButton Rot;
		public FlystickCursor Crs;
	}
}