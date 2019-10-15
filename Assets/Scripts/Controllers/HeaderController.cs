using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers {
    public class HeaderController : MonoBehaviour {

        public GameObject Entity;
        public float HeaderHeight = 8f;
        public float HeaderDistance = 18f;
        public string CurrentlySelectedText = "Selected:";
        public string CurrentlyLookingAtText = "Looking at:";

        private InputController inputController;
        private NodeController nodeController;

        void Awake() {
            nodeController = GetComponent<NodeController>();
            inputController = GetComponent<InputController>();
        }
        
        void Update() {
            // Header text and position
            GameObject headerObject = GameObject.Find("Header");
            TextMesh headerTitle = headerObject.transform.GetChild(0).GetComponent<TextMesh>();
            TextMesh headerValue = headerObject.transform.GetChild(1).GetComponent<TextMesh>();
            Vector3 targetPosition;

            if (nodeController.SelectedNode != null) {
                targetPosition = GraphController.Graph.NodeObjectMap[nodeController.SelectedNode].transform.TransformPoint(new Vector3(0, HeaderHeight, 0));
                headerTitle.text = CurrentlySelectedText;
                headerValue.text = nodeController.SelectedNode.Title;
            }
            if (nodeController.HighlightedNode != null) {
                targetPosition = Entity.transform.position;
                headerTitle.text = CurrentlyLookingAtText;
                headerValue.text = nodeController.HighlightedNode.Title;
            } else {
                if (nodeController.SelectedNode != null) {
                    targetPosition = GraphController.Graph.NodeObjectMap[nodeController.SelectedNode].transform.TransformPoint(new Vector3(0, HeaderHeight, 0));
                    headerTitle.text = CurrentlySelectedText;
                    headerValue.text = nodeController.SelectedNode.Title;
                } else {
                    targetPosition = Entity.transform.position;
                    headerTitle.text = "";
                    headerValue.text = "";
                }
            }

            if (inputController.Environment == Environment.Cave) {
                headerObject.transform.position = targetPosition + new Vector3(Mathf.Sin(Entity.transform.rotation.eulerAngles.y / 180f * Mathf.PI) * HeaderDistance, 0, Mathf.Cos(Entity.transform.rotation.eulerAngles.y / 180f * Mathf.PI) * HeaderDistance);
            } else {
                headerObject.transform.parent = Camera.main.transform;
                headerObject.transform.localPosition = Vector3.forward * 2 * HeaderDistance;
            }
            headerObject.transform.LookAt(headerObject.transform.position + Entity.transform.rotation * Vector3.forward, Entity.transform.rotation * Vector3.up);
        }
    }
}