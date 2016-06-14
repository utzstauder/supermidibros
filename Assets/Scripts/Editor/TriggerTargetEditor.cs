using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TriggerTarget), true)]
public class TriggerTargetEditor : Editor {

	public override void OnInspectorGUI (){
		EditorGUILayout.HelpBox("Every Trigger inside the Triggers Array will trigger the action in this script.", MessageType.Info);
		DrawDefaultInspector();
	}
}
