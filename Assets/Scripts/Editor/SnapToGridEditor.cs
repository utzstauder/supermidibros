using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SnapToGrid))]
public class SnapToGridEditor : Editor {

	bool[,] grid = new bool[Constants.NUMBER_OF_PLAYERS, Constants.VERTICAL_POSITIONS];
		
	public override void OnInspectorGUI ()
	{
		SnapToGrid script = (SnapToGrid)target;

		GUILayout.BeginHorizontal();

		GUILayout.Label("Snap X");
		script.m_snapX = EditorGUILayout.Toggle(script.m_snapX);
		GUILayout.Label("Snap Y");
		script.m_snapY = EditorGUILayout.Toggle(script.m_snapY);
		GUILayout.Label("Snap Z");
		script.m_snapZ = EditorGUILayout.Toggle(script.m_snapZ);

		GUILayout.EndHorizontal();


		if (script.m_snapX){
			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

			EditorGUILayout.BeginHorizontal();

			GUILayout.Label("X (time)", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical();
			GUILayout.Label("Bar");
			script.m_bar = EditorGUILayout.IntField(script.m_bar);
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			GUILayout.Label("Beat");
			script.m_beat = EditorGUILayout.IntField(script.m_beat);
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			GUILayout.Label("SubBeat");
			script.m_subBeat = EditorGUILayout.IntField(script.m_subBeat);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
		}

		if (script.m_snapY && script.m_snapZ){
			// draw table
			float cellWidth = EditorGUIUtility.currentViewWidth / ((float)grid.GetLength(0) * 3/2);

			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Y&Z", EditorStyles.boldLabel, GUILayout.Width(cellWidth));
			// label row
			for (int x = 0; x < grid.GetLength(0); x++){
				GUILayout.Label("" + x, GUILayout.Width(cellWidth));
			}
			EditorGUILayout.EndHorizontal();

			for (int y = grid.GetLength(1) - 1; y >= 0; y--){
				EditorGUILayout.BeginHorizontal();

				GUILayout.Label("" + y, GUILayout.Width(cellWidth)); // label column

				for (int x = 0; x < grid.GetLength(0); x++){
					string buttonText = (script.m_playerLane == x && script.m_verticalPosition == y) ? "X": " ";
					if (GUILayout.Button(buttonText, GUILayout.Width(cellWidth))){
						Undo.RecordObject(target, "SnapToGrid SetPosition");
						script.SetPosition(x, y);
						EditorUtility.SetDirty(target);
					}			
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();

		} else if (script.m_snapY){
			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

			EditorGUILayout.BeginHorizontal();

			GUILayout.Label("Y (vertical position)", EditorStyles.boldLabel);
			script.m_verticalPosition = EditorGUILayout.IntSlider(script.m_verticalPosition, 0, Constants.VERTICAL_POSITIONS-1);

			EditorGUILayout.EndHorizontal();
		} else if (script.m_snapZ){
			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

			EditorGUILayout.BeginHorizontal();

			GUILayout.Label("Z (player lane)", EditorStyles.boldLabel);
			script.m_playerLane = EditorGUILayout.IntSlider(script.m_playerLane, 0, Constants.NUMBER_OF_PLAYERS-1);

			EditorGUILayout.EndHorizontal();
		}

		if (script.m_snapX || script.m_snapY || script.m_snapZ){
			Undo.RecordObject(target, "SnapToGrid");
			script.UpdatePosition();
			EditorUtility.SetDirty(target);
		}
	}
}
