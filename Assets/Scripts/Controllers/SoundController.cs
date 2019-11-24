using Controllers;
using InputModule.Processor;
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
	bool isInfo = true;

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
		actionController.playSelectSound += (oldNode, newNode, source, isUndo) => {

			if (source == ActionController.NodeChangedSource.History) PlayStickySoundSelected(SoundType.HISTORY, isUndo ? 0 : 1);
			else if (newNode != null && oldNode != null) PlayStickySoundRandom(SoundType.NODE_SELECTED);
			else PlayStickySoundSelected(SoundType.FREE_FLIGHT, newNode == null ? 0 : 1);
			
		};
		graphController.ConnectionMode.OnValueChanged += mode => PlayStickySoundSelected(SoundType.MODE, mode == ConnectionMode.CHILDREN ? 0 : 1);
		connectionController.OnScrollInDirection += PlayScrollSounds;
		InputProcessor.playInfoSound += () => {
			PlayStickySoundSelected(SoundType.INFO, isInfo ? 0 : 1);
			isInfo = !isInfo;
		};
	}

	public void PlayScrollSounds(int dir) {
		if (dir == 1) {
			if (scrollSoundPositionUp == 4) { //fix 
				scrollSoundPositionUp = 0;
			}
			PlayStickySoundSelected(SoundType.SCROLLED, scrollSoundPositionUp);
			scrollSoundPositionUp++;
		}
		else if (dir == -1) {
			if (scrollSoundPositionDown == -1) {
				scrollSoundPositionDown = 3;
			}

			PlayStickySoundSelected(SoundType.SCROLLED, scrollSoundPositionDown);
			scrollSoundPositionDown--;
		}
		else {
			scrollSoundPositionDown = 3;
			scrollSoundPositionUp = 0;
		}


	}


	private void PlaySpacialNodeSoundRandom(Node node, SoundType sound) {
		Vector3 direction = GetNodePosition(node) - player.position;
		AudioSource.PlayClipAtPoint(soundManager.GetRandom(sound), player.position + direction * (MaxNodeSpacialSoundDist / graphController.WorldRadius));
	}

	private void PlaySpacialNodeSoundSelected(Node node, SoundType sound, int index) {
		Vector3 direction = GetNodePosition(node) - player.position;
		AudioSource.PlayClipAtPoint(soundManager.Get(sound, index), player.position + direction * (MaxNodeSpacialSoundDist / graphController.WorldRadius));
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
