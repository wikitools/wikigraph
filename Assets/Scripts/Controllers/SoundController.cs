using Controllers;
using Model;
using UnityEngine;

public class SoundController : MonoBehaviour {
	public GameObject Graph;
	[Range(1, 50)]
	public float MaxNodeSpacialSoundDist = 10f;
	
	private AudioSource Audio;
	private Transform player;
	
	private NodeController nodeController;
	private GraphController graphController;
	
	void Start () {
		if(Graph.GetComponent<NetworkController>().IsClient())
			return;
		player = Graph.GetComponent<InputController>().Eyes.transform;
		transform.parent = player;
		Audio = GetComponent<AudioSource>();
		
		nodeController = Graph.GetComponent<NodeController>();
		graphController = Graph.GetComponent<GraphController>();
		nodeController.OnSelectedNodeChanged += (oldNode, newNode) => PlaySpacialNodeSound(newNode, null);

	}

	private void PlaySpacialNodeSound(Node node, AudioClip sound) {
		Vector3 direction = GetNodePosition(node) - player.position;
		AudioSource.PlayClipAtPoint(sound, player.position + direction * (MaxNodeSpacialSoundDist / graphController.WorldRadius));
	}

	private Vector3 GetNodePosition(Node node) => GraphController.Graph.NodeObjectMap[node].transform.position;

}
