using System;
using System.Collections.Generic;

namespace Inspector {
	[AttributeUsage(AttributeTargets.Field)]
	public class NotBlocked : Attribute {
		public readonly List<InputBlockType> NotBlockedTypes;
		public NotBlocked(params InputBlockType[] notBlockedTypes) {
			NotBlockedTypes = new List<InputBlockType>(notBlockedTypes);
		}
	}

	public enum InputBlockType {
		CONSOLE, INFO_SPACE
	}
}