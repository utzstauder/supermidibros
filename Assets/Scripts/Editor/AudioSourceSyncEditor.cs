using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AudioSourceSync))]
public class AudioSourceSyncEditor : Editor {

	public override void OnInspectorGUI ()
	{
		AudioSourceSync script = (AudioSourceSync)target;

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("IsLoop");
		script.m_isLoop = EditorGUILayout.Toggle(script.m_isLoop);
		EditorGUILayout.EndHorizontal();

		if (script.m_isLoop){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("ManualLoopArea");
			script.m_manualLoopArea = EditorGUILayout.Toggle(script.m_manualLoopArea);
			EditorGUILayout.EndHorizontal();

			if (script.m_manualLoopArea){
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("LoopLength (bars)");
				script.m_loopLength = EditorGUILayout.IntField(script.m_loopLength);
				script.m_loopLength = Mathf.Clamp(script.m_loopLength, 1, script.m_loopLength);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("LoopOffset (bars)");
				script.m_loopOffset = EditorGUILayout.IntField(script.m_loopOffset);
				script.m_loopOffset = Mathf.Clamp(script.m_loopOffset, 0, script.m_loopOffset);
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}
