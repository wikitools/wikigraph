using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Services {
	public class SoundManager {
		private readonly Dictionary<SoundType, AudioClip[]> Sounds = new Dictionary<SoundType, AudioClip[]>();
		private const string PREFIX = "Sounds";
		private readonly Random random;

		public SoundManager() {
			random = new Random();
			
			Register(SoundType.AMBIENT_LOOP, "WikiAmbient");
			Register(SoundType.NODE_SELECTED, "select1", "selectandscroll2", "selectandscroll3", "selectandscroll4");
			Register(SoundType.SCROLLED, "scroll1", "selectandscroll2", "selectandscroll3", "selectandscroll4");
			Register(SoundType.HISTORY, "historiaUP", "historiaDOWN");
			Register(SoundType.INFO, "infoOUT", "infoIN");
			Register(SoundType.MODE, "mode1", "mode2");
			Register(SoundType.FREE_FLIGHT, "freeIN", "freeOUT");
		}

		public AudioClip GetRandom(SoundType type) => Sounds[type][random.Next(Sounds[type].Length)];

		public AudioClip Get(SoundType type, int index) => Sounds[type][index];

		private void Register(SoundType type, params string[] files) {
			Sounds.Add(type, files.Select(LoadFile).ToArray());
		}

		private AudioClip LoadFile(string file) => Resources.Load<AudioClip>(Path.Combine(PREFIX, file));
	}
		
	public enum SoundType {
		AMBIENT_LOOP,
		SCROLLED,
		NODE_SELECTED,
		HISTORY,
		INFO,
		MODE,
		FREE_FLIGHT
	}
}