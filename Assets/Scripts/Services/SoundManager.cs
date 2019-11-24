using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Services {
	public class SoundManager {
		public readonly Dictionary<SoundType, AudioClip> Sounds = new Dictionary<SoundType, AudioClip>();
		private const string PREFIX = "Sounds";

		public SoundManager() {
			Register(SoundType.AMBIENT_LOOP, "WikiAmbient");
			Register(SoundType.NODE_SELECTED1, "select1");
			Register(SoundType.SCROLLED1, "scroll1");
			Register(SoundType.NODE_SELECTED_SCROLLED2, "selectandscroll2");
			Register(SoundType.NODE_SELECTED_SCROLLED3, "selectandscroll3");
			Register(SoundType.NODE_SELECTED_SCROLLED4, "selectandscroll4");
			Register(SoundType.HISTORY_UP, "historyUP");
			Register(SoundType.HISTORY_DOWN, "historyDOWN");
			Register(SoundType.INFO_IN, "infoIN");
			Register(SoundType.INFO_OUT, "infoOUT");
			Register(SoundType.MODE1, "mode1");
			Register(SoundType.MODE2, "mode2");
		}

		private void Register(SoundType type, string file) {
			Sounds.Add(type, Resources.Load<AudioClip>(Path.Combine(PREFIX, file)));
		}
	}
		
	public enum SoundType {
		AMBIENT_LOOP,
		NODE_SELECTED1,
		SCROLLED1,
		NODE_SELECTED_SCROLLED2,
		NODE_SELECTED_SCROLLED3,
		NODE_SELECTED_SCROLLED4,
		HISTORY_UP,
		HISTORY_DOWN,
		INFO_IN,
		INFO_OUT,
		MODE1,
		MODE2
	}
}