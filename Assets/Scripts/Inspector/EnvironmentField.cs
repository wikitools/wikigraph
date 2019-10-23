using System;
using Environment = Controllers.Environment;

namespace Inspector {
	[AttributeUsage(AttributeTargets.Field)]
	public class EnvironmentField: Attribute {
		public Environment TargetEnvironment { get; private set; }

		public EnvironmentField(Environment targetEnvironment) {
			TargetEnvironment = targetEnvironment;
		}
	}
}