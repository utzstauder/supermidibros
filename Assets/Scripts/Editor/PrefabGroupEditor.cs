using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PrefabGroup))]
public class NewBehaviourScript : Editor {

	public override void OnInspectorGUI(){
		DrawDefaultInspector();

		PrefabGroup script = (PrefabGroup)target;

		GUILayout.BeginHorizontal();

			if(GUILayout.Button("Instantiate Prefabs")){
				script.InstantiatePrefabs(script.m_instances);
			}
			if(GUILayout.Button("Clear Prefabs")){
				script.ClearPrefabs();
			}

		GUILayout.EndHorizontal();

	}
}
