using Controllers;
using Model;
using Services;
using UnityEngine;

public class SoundController : MonoBehaviour {
	public GameObject Graph;
	[Range(1, 50)]
	public float MaxNodeSpacialSoundDist = 10f;
	
	private SoundManager SoundManager;
	private Transform player;
	
	private NodeController nodeController;
	private GraphController graphController;
	
	void Start () {
		if(Graph.GetComponent<NetworkController>().IsClient())
			return;
		player = Graph.GetComponent<InputController>().Eyes.transform;
		transform.parent = player;
		
		SoundManager = new SoundManager();
		var Audio = GetComponent<AudioSource>();
		Audio.clip = SoundManager.Sounds[SoundType.AMBIENT_LOOP];
		Audio.Play();
		
		nodeController = Graph.GetComponent<NodeController>();
		graphController = Graph.GetComponent<GraphController>();
		nodeController.OnSelectedNodeChanged += (oldNode, newNode) => PlayLocalSound(SoundType.NODE_SELECTED);
	}

	private void PlaySpacialNodeSound(Node node, SoundType sound) {
		Vector3 direction = GetNodePosition(node) - player.position;
		AudioSource.PlayClipAtPoint(SoundManager.Sounds[sound], player.position + direction * (MaxNodeSpacialSoundDist / graphController.WorldRadius));
	}

	private void PlayLocalSound(SoundType sound) {
		AudioSource.PlayClipAtPoint(SoundManager.Sounds[sound], player.position);
	}

	private Vector3 GetNodePosition(Node node) => GraphController.Graph.NodeObjectMap[node].transform.position;

}
