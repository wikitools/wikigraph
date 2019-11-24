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
	List<SoundType> bothSounds = new List<SoundType> { SoundType.NODE_SELECTED_SCROLLED2, SoundType.NODE_SELECTED_SCROLLED3, SoundType.NODE_SELECTED_SCROLLED4 };
	List<SoundType> selectedSounds = new List<SoundType>();
	List<SoundType> scrolledSounds = new List<SoundType>();
	private NodeController nodeController;
	private GraphController graphController;
	private ActionController actionController;
	private ConnectionController connectionController;

	int scrollSoundPositionUp = 0;
	int scrollSoundPositionDown = 3;

	void Start() {
		selectedSounds.AddRange(bothSounds);
		selectedSounds.Add(SoundType.NODE_SELECTED1);
		scrolledSounds.Add(SoundType.SCROLLED1);
		scrolledSounds.AddRange(bothSounds);
		if (Graph.GetComponent<NetworkController>().IsClient()) {
			return;
		}

		player = Graph.GetComponent<InputController>().Eyes.transform;
		transform.parent = player;
		soundManager = new SoundManager();
		var Audio = GetComponent<AudioSource>();
		Audio.clip = soundManager.Sounds[SoundType.AMBIENT_LOOP];
		Audio.Play();

		nodeController = Graph.GetComponent<NodeController>();
		graphController = Graph.GetComponent<GraphController>();
		actionController = Graph.GetComponent<ActionController>();
		connectionController = Graph.GetComponent<ConnectionController>();
		nodeController.OnSelectedNodeChanged += (oldNode, newNode) => {
			if (actionController.nodeChangedSource != ActionController.NodeChangedSource.History) {
				PlayStickySound(selectedSounds[Random.Range(0, selectedSounds.Count - 1)]);
			}
		};
		graphController.ConnectionMode.OnValueChanged += (mode) => {
			if (mode == ConnectionMode.CHILDREN) {
				PlayLocalSound(SoundType.MODE1);
			}
			else {
				PlayLocalSound(SoundType.MODE2);
			}
		};
		connectionController.OnConnectionRangeChanged += (start, end, count) => PlayScrollSounds(start, end);

	}

	public void PlayScrollSounds(int start, int end) {
		if (start != 0 && end != 0) {
			if (1 == 1) {
				if (scrollSoundPositionUp == scrolledSounds.Count) {
					scrollSoundPositionUp = 0;
				}

				PlayLocalSound(scrolledSounds[scrollSoundPositionUp]);
				scrollSoundPositionUp++;
			}
			else if (start == -1) {
				if (scrollSoundPositionDown == -1) {
					scrollSoundPositionDown = 0;
				}

				PlayLocalSound(scrolledSounds[scrollSoundPositionDown]);
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
		AudioSource.PlayClipAtPoint(soundManager.Sounds[sound], player.position + direction * (MaxNodeSpacialSoundDist / graphController.WorldRadius));
	}

	private void PlayLocalSound(SoundType sound) {
		AudioSource.PlayClipAtPoint(soundManager.Sounds[sound], player.position);
	}

	private void PlayStickySound(SoundType sound) {
		var audioSource = sources.FirstOrDefault(source => !source.isPlaying);
		if (audioSource == null) {
			audioSource = gameObject.AddComponent<AudioSource>();
			sources.Add(audioSource);
		}
		audioSource.PlayOneShot(soundManager.Sounds[sound]);
	}

	private Vector3 GetNodePosition(Node node) => GraphController.Graph.NodeObjectMap[node].transform.position;

}
