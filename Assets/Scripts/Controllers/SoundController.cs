using System.Collections.Generic;
using System.Linq;
using Controllers;
using Model;
using Services;
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
	
	void Start () {
		if(Graph.GetComponent<NetworkController>().IsClient())
			return;
		player = Graph.GetComponent<InputController>().Eyes.transform;
		transform.parent = player;
		
		soundManager = new SoundManager();
		var Audio = GetComponent<AudioSource>();
		Audio.clip = soundManager.Sounds[SoundType.AMBIENT_LOOP];
		Audio.Play();
		
		nodeController = Graph.GetComponent<NodeController>();
		graphController = Graph.GetComponent<GraphController>();
		nodeController.OnSelectedNodeChanged += (oldNode, newNode) => PlayStickySound(SoundType.NODE_SELECTED);
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
