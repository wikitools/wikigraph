using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers;
using Services.DataFiles;
using UnityEditor;
using UnityEngine;

namespace Services {
#if UNITY_EDITOR
	[CustomEditor(typeof(NodeController))]
	public class NodeInspector: Editor {
		private readonly string DATE_FORMAT = "yyyyMMdd";
		
		private List<string> dataPacks = new List<string>();
		private List<string> dataPackDates = new List<string>();
		
		private SerializedProperty dataPack;
		private SerializedProperty dataPackDate;
		private SerializedObject script;
		private GUIStyle labelStyle = new GUIStyle();
		
		private void OnEnable() {
			script = new SerializedObject(target);
			dataPack = script.FindProperty("DataPack");
			dataPackDate = script.FindProperty("DataPackDate");
			ScanDataPacks();
			labelStyle.alignment = TextAnchor.MiddleCenter;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if (EditorApplication.isPlaying) 
				GUI.enabled = false;
			
			if (dataPacks.Count == 0)
				EditorGUILayout.LabelField("No Data Packs found", labelStyle);
			else {
				var index = dataPacks.IndexOf(dataPack.stringValue);
				int selectedDataPack = EditorGUILayout.Popup("Data Pack", Mathf.Clamp(index, 0, dataPacks.Count), dataPacks.ToArray());
				dataPack.stringValue = dataPacks[selectedDataPack];
				dataPackDate.stringValue = dataPackDates[selectedDataPack];
			}
			if (GUILayout.Button("Reload Data Packs"))
				ScanDataPacks();
			GUI.enabled = true;
			script.ApplyModifiedProperties();
		}

		private void ScanDataPacks() {
			dataPacks = GetDirectories(DataFileReader.DATA_FILE_PATH);
			dataPackDates = dataPacks.Select(pack => GetDirectories(Path.Combine(DataFileReader.DATA_FILE_PATH, pack))
				.Select(ToDate).Max(d => d).ToString(DATE_FORMAT)).ToList();
		}
		
		private List<string> GetDirectories(string path) => Directory.GetDirectories(path).Select(Path.GetFileName).ToList();
		private DateTime ToDate(string date) => DateTime.ParseExact(date, DATE_FORMAT, null);
	}
#endif
}