using Controllers;
using Model;
using Services;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundController : MonoBehaviour {
	public GameObject Graph;
	[Range(1, 50)]
	public float MaxNodeSpacialSoundDist = 10f;

	private List<AudioSource> sources = new List<AudioSource>();
	private SoundManager soundManager;
	private Transform player;
	private NodeController nodeController;
	private GraphController graphController;
	private ActionController actionController;
	private ConnectionController connectionController;

	int scrollSoundPositionUp = 0;
	int scrollSoundPositionDown = 3;

	void Start() {
		if (Graph.GetComponent<NetworkController>().IsClient()) {
			return;
		}

		player = Graph.GetComponent<InputController>().Eyes.transform;
		transform.parent = player;
		soundManager = new SoundManager();
		var Audio = GetComponent<AudioSource>();
		Audio.clip = soundManager.GetRandom(SoundType.AMBIENT_LOOP);
		Audio.Play();

		nodeController = Graph.GetComponent<NodeController>();
		graphController = Graph.GetComponent<GraphController>();
		actionController = Graph.GetComponent<ActionController>();
		connectionController = Graph.GetComponent<ConnectionController>();
		nodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
			if (actionController.nodeChangedSource != ActionController.NodeChangedSource.History) {
				PlayStickySoundRandom(SoundType.NODE_SELECTED);
			}
		};
		graphController.ConnectionMode.OnValueChanged += (mode) => {
			if (mode == ConnectionMode.CHILDREN) {
				PlayLocalSoundSelected(SoundType.MODE, 0);
			}
			else {
				PlayLocalSoundSelected(SoundType.MODE, 1);
			}
		};
		connectionController.OnConnectionRangeChanged += (start, end, count) => PlayScrollSounds(start, end);

	}

	public void PlayScrollSounds(int start, int end) {
		if (start != 0 && end != 0) {
			if (1 == 1) {
				if (scrollSoundPositionUp == 4) { //fix 
					scrollSoundPositionUp = 0;
				}
				PlayLocalSoundSelected(SoundType.SCROLLED, scrollSoundPositionUp);
				scrollSoundPositionUp++;
			}
			else if (start == -1) {
				if (scrollSoundPositionDown == -1) {
					scrollSoundPositionDown = 0;
				}

				PlayLocalSoundSelected(SoundType.SCROLLED, scrollSoundPositionDown);
				scrollSoundPositionDown--;
			}
			else {
				scrollSoundPositionDown = 3;
				scrollSoundPositionUp = 0;
			}
		}

	}


	private void PlaySpacialNodeSound(Node node, SoundType sound) {
		Vector3 direction = GetNodePosition(node) - player.position;
		AudioSource.PlayClipAtPoint(soundManager.GetRandom(sound), player.position + direction * (MaxNodeSpacialSoundDist / graphController.WorldRadius));
	}

	private void PlayLocalSoundRandom(SoundType sound) {
		AudioSource.PlayClipAtPoint(soundManager.GetRandom(sound), player.position);
	}

	private void PlayLocalSoundSelected(SoundType sound, int index) {
		AudioSource.PlayClipAtPoint(soundManager.Get(sound, index), player.position);
	}

	private void PlayStickySoundRandom(SoundType sound) {
		var audioSource = GetAudioSource();
		audioSource.PlayOneShot(soundManager.GetRandom(sound));
	}

	private void PlayStickySoundSelected(SoundType sound, int index) {
		var audioSource = GetAudioSource();
		audioSource.PlayOneShot(soundManager.Get(sound, index));
	}

	private AudioSource GetAudioSource() {
		var audioSource = sources.FirstOrDefault(source => !source.isPlaying);
		if (audioSource == null) {
			audioSource = gameObject.AddComponent<AudioSource>();
			sources.Add(audioSource);
		}
		return audioSource;
	}

	private Vector3 GetNodePosition(Node node) => GraphController.Graph.NodeObjectMap[node].transform.position;

}
