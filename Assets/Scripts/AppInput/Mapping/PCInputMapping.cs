using AppInput.Event.Button;
using AppInput.Event.Cursor;

namespace AppInput.Mapping {
	[System.Serializable]
	public class PCInputMapping: InputMapping {
		public PCCursor RotationInput;
		public PCButton Btn;
	}
}