using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TriggerGroup))]
public class TriggerGroupEditor : Editor {

	public override void OnInspectorGUI ()
	{
		EditorGUILayout.HelpBox("This TriggerGroup will trigger once _every_ Trigger in the Triggers array has been triggered. Lot's of triggerin' going on here.", MessageType.Info);
		DrawDefaultInspector();
	}
}
