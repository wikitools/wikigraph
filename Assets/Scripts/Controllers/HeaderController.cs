﻿using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Model;

namespace Controllers {
    public class HeaderController : MonoBehaviour {

        public GameObject Entity;
        public Canvas PCCanvas;
        public float HeaderHeight = -6f;
        public float HeaderDeviation = -10f;
        public float HeaderDistance = 16f;
        public string CurrentlySelectedText = "Selected:";
        public string CurrentlyLookingAtText = "Looking at:";
        private GameObject headerObject;
        private Vector3 targetPosition;

        private InputController inputController;
        private NodeController nodeController;

        void Awake() {
            headerObject = GameObject.Find("Header");
            inputController = GetComponent<InputController>();
            nodeController = GetComponent<NodeController>();
            nodeController.OnSelectedNodeChanged += UpdateNodeHeaderAfterSelectOrHightlight;
            nodeController.OnHighlightedNodeChanged += UpdateNodeHeaderAfterSelectOrHightlight;
        }

        private void UpdateNodeHeaderAfterSelectOrHightlight(Node previousNode, Node selectedNode) {
            TextMesh headerTitle = headerObject.transform.GetChild(0).GetComponent<TextMesh>();
            TextMesh headerValue = headerObject.transform.GetChild(1).GetComponent<TextMesh>();
            if (nodeController.HighlightedNode != null) {
                targetPosition = Entity.transform.position;
                headerTitle.text = CurrentlyLookingAtText;
                headerValue.text = nodeController.HighlightedNode.Title;
            } else {
                targetPosition = Entity.transform.position;
                headerTitle.text = "";
                headerValue.text = "";
            }
            if (nodeController.SelectedNode != null) {
                if (nodeController.HighlightedNode != null && nodeController.HighlightedNode != nodeController.SelectedNode) {
                    targetPosition = GraphController.Graph.NodeObjectMap[nodeController.SelectedNode].transform.position;
                    headerTitle.text = CurrentlyLookingAtText;
                    headerValue.text = nodeController.HighlightedNode.Title;
                } else {
                    targetPosition = GraphController.Graph.NodeObjectMap[nodeController.SelectedNode].transform.position;
                    headerTitle.text = CurrentlySelectedText;
                    headerValue.text = nodeController.SelectedNode.Title;
                }
            }
        }

        void Update() {
            if (nodeController.HighlightedNode != null && nodeController.SelectedNode == null) {
                targetPosition = Entity.transform.position;
            }
            if (inputController.Environment == Environment.Cave) {
                headerObject.transform.position = targetPosition + new Vector3(Mathf.Sin(Entity.transform.rotation.eulerAngles.y / 180f * Mathf.PI) * HeaderDistance, HeaderHeight, Mathf.Cos(Entity.transform.rotation.eulerAngles.y / 180f * Mathf.PI) * HeaderDistance);
                headerObject.transform.rotation = Quaternion.LookRotation(headerObject.transform.position - (targetPosition + new Vector3(0, HeaderDeviation, 0)));
            } else {
                headerObject.transform.parent = Camera.main.transform;
                headerObject.transform.localPosition = Vector3.forward * 2 * HeaderDistance;
            }
        }

    }
}