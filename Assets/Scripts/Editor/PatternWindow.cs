using UnityEngine;
using UnityEditor;
using System.Collections;

public class PatternWindow : EditorWindow {

	bool[,]	grid = new bool[Constants.NUMBER_OF_PLAYERS, Constants.VERTICAL_POSITIONS + 1];
	int[]	selected = new int[Constants.VERTICAL_POSITIONS + 1];

	const string prefabPrefix		= "Pattern_";
	const string prefabParentFolder = "Assets/Prefabs";
	const string prefabFolder		= "Patterns";

	GameObject childPrefab;

	[MenuItem("Window/Pattern Prefab Generator %g")]
	static void Init(){
		PatternWindow window = (PatternWindow)EditorWindow.GetWindow(typeof(PatternWindow));
		window.Show();
	}

	void OnGUI(){
		DrawTable();

		DrawPrefabCreator();
	}


	#region gui draw functions

	void DrawTable(){
		GUILayout.Label("Pattern Generator", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();

		for (int x = 0; x < grid.GetLength(0); x++){
			
			string[] selectionGridStrings = new string[Constants.VERTICAL_POSITIONS + 1];
			for (int y = 0; y < selectionGridStrings.Length; y++){
				if (y == 0){
					selectionGridStrings[selectionGridStrings.Length - 1 - y] = "none";
				} else {
					selectionGridStrings[selectionGridStrings.Length - 1 - y] = x + ", " + (y - 1);
				}
			}

			selected[x] = Constants.VERTICAL_POSITIONS -
				GUILayout.SelectionGrid(
				//new Rect((cellSize + cellPadding) * x, 0, cellSize, (cellSize + cellPadding) * Constants.VERTICAL_POSITIONS),
				selected[x],
				selectionGridStrings,
				1);

			SetActiveCell(x, selected[x]);

//			for (int x = 0; x < grid.GetLength(0); x++){
//				// TODO: cells
//				//SetActiveCell(x, y);
//			}


		}

		EditorGUILayout.EndHorizontal();
	}


	void DrawPrefabCreator(){
		GUILayout.Label("Prefab Creator", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Trigger Prefab:");
		childPrefab = (GameObject)EditorGUILayout.ObjectField(childPrefab, typeof(GameObject), false);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Create Prefab: " + GetPrefabName())){
			CreatePrefab();
		}
		EditorGUILayout.EndHorizontal();
	}

	#endregion


	#region functions

	void SetActiveCell(int x, int y){
		//Debug.Log("SetActiveCell(" + x + ", " + y + ")");
		for (int i = 0; i < Constants.VERTICAL_POSITIONS + 1; i++){
			grid[x, grid.GetLength(1) - 1 - y] = (i == y);
		}
	}

	string GetPrefabName(){
		string name = prefabPrefix;

		for (int x = 0; x < Constants.NUMBER_OF_PLAYERS; x++){
			name += (selected[x] > 0) ? (selected[x] - 1).ToString() : "x";
		}

		return name;
	}

	void CreatePrefab(){
		if (!childPrefab){
			Debug.LogError("No ChildPrefab set!");
			return;
		}

		GameObject gameObject = new GameObject();
		gameObject.name = GetPrefabName();

		// create children
		for (int x = 0; x < Constants.NUMBER_OF_PLAYERS; x++){
			if (selected[x] > 0){
				GameObject child = PrefabUtility.InstantiatePrefab(childPrefab) as GameObject;
				child.GetComponent<SnapToGrid>().SetPosition(x, selected[x] - 1);
				child.GetComponent<SnapToGrid>().m_lockX = true;
				child.transform.parent = gameObject.transform;
			}
		}

		// check if there are any children at all
		if (gameObject.transform.childCount <= 0){
			DestroyImmediate(gameObject);
			Debug.LogError("Can not create an empty TriggerGroup you cunt!");
			return;
		}

		// attach scripts to object
		SnapToGrid snapToGrid = gameObject.AddComponent<SnapToGrid>();
		snapToGrid.m_snapX = true;
		snapToGrid.m_snapY = false;
		snapToGrid.m_snapZ = false;
		snapToGrid.m_lockY = true;
		snapToGrid.m_lockZ = true;

		TriggerGroup triggerGroup = gameObject.AddComponent<TriggerGroup>();
		triggerGroup.m_moveChildrenAlongXAxis = true;
		triggerGroup.m_lookForTriggersInChildren = true;

		// create folder
		if (!AssetDatabase.IsValidFolder(prefabParentFolder + "/" + prefabFolder)){
			AssetDatabase.CreateFolder(prefabParentFolder, prefabFolder);
		}

		// create prefab
		PrefabUtility.CreatePrefab(prefabParentFolder + "/" + prefabFolder + "/" + GetPrefabName() + ".prefab", gameObject, ReplacePrefabOptions.Default);

		// clean up
		DestroyImmediate(gameObject);
	}

	#endregion
}
