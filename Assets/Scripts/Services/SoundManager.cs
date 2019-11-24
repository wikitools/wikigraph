using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Services {
	public class SoundManager {
		public readonly Dictionary<SoundType, AudioClip> Sounds = new Dictionary<SoundType, AudioClip>();
		private const string PREFIX = "Sounds";

		public SoundManager() {
			Register(SoundType.AMBIENT_LOOP, "loop");
			Register(SoundType.NODE_SELECTED, "selected");
		}

		private void Register(SoundType type, string file) {
			Sounds.Add(type, Resources.Load<AudioClip>(Path.Combine(PREFIX, file)));
		}
	}
		
	public enum SoundType {
		AMBIENT_LOOP,
		NODE_SELECTED
	}
}