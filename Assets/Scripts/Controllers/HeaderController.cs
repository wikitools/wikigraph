using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Model;

namespace Controllers {
    public class HeaderController : MonoBehaviour {

        public GameObject Entity;
        public GameObject HeaderObject;
        
        public float HeaderHeight = -6f;
        public float HeaderDeviation = -10f;
        public float HeaderDistance = 16f;
        public string CurrentlySelectedText = "Selected:";
        public string CurrentlyLookingAtText = "Looking at:";

        private Vector3 targetPosition;
        private InputController inputController;
        private NodeController nodeController;
        private NetworkController networkController;

        void Start() {
            if (inputController.Environment == Environment.PC) {
                HeaderObject.transform.parent = Camera.main.transform;
            }
            nodeController.OnSelectedNodeChanged += UpdateNodeHeaderAfterSelectOrHighlight;
            nodeController.OnHighlightedNodeChanged += UpdateNodeHeaderAfterSelectOrHighlight;
        }

        void Awake() {
            networkController = GetComponent<NetworkController>();
            inputController = GetComponent<InputController>();
            nodeController = GetComponent<NodeController>();
        }

        private void UpdateNodeHeaderAfterSelectOrHighlight(Node previousNode, Node selectedNode) {
            TextMesh headerTitle = HeaderObject.transform.GetChild(0).GetComponent<TextMesh>();
            TextMesh headerValue = HeaderObject.transform.GetChild(1).GetComponent<TextMesh>();

            if (nodeController.HighlightedNode != null) {
                headerTitle.text = CurrentlyLookingAtText;
                headerValue.text = nodeController.HighlightedNode.Title;
            } else {
                headerTitle.text = "";
                headerValue.text = "";
            }
            if (nodeController.SelectedNode != null) {
                if (nodeController.HighlightedNode != null && nodeController.HighlightedNode != nodeController.SelectedNode) {
                    headerTitle.text = CurrentlyLookingAtText;
                    headerValue.text = nodeController.HighlightedNode.Title;
                } else {
                    headerTitle.text = CurrentlySelectedText;
                    headerValue.text = nodeController.SelectedNode.Title;
                }
            }
        }

        void Update() {
            if (networkController.IsClient()) {
                return;
            }

            targetPosition = Entity.transform.position;
            if (inputController.Environment == Environment.Cave) {
                HeaderObject.transform.position = targetPosition + new Vector3(Mathf.Sin(Entity.transform.rotation.eulerAngles.y / 180f * Mathf.PI) * HeaderDistance, HeaderHeight, Mathf.Cos(Entity.transform.rotation.eulerAngles.y / 180f * Mathf.PI) * HeaderDistance);
                HeaderObject.transform.rotation = Quaternion.LookRotation(HeaderObject.transform.position - (targetPosition + new Vector3(0, HeaderDeviation, 0)));
            } else {
                HeaderObject.transform.localPosition = Vector3.forward * 2 * HeaderDistance;
            }
        }

    }
}