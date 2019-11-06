using System;

namespace Inspector {
	[AttributeUsage(AttributeTargets.Field)]
	public class UnblockableEvent: Attribute {}
}