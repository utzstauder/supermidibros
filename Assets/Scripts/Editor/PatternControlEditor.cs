using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Collections;
using CustomDataTypes;

[CustomEditor(typeof(PatternControll))]
public class PatternControlEditor : Editor {

	bool dataOptions = false;

	bool randomOptions	= false;
	bool symmetrical	= false;
	int patternSize		= 8;

	int[] selectedCoords = new int[Constants.NUMBER_OF_PLAYERS];

	int selectedCategory	= 0;
	int selectedInstrument	= 0;
	int selectedVariation	= 0;

	public override void OnInspectorGUI ()
	{
		if (Application.isPlaying){
			DrawDefaultInspector();
		} else {
			
		PatternControll script = (PatternControll)target;

		dataOptions = EditorGUILayout.Foldout(dataOptions, "Data Options");
		if (dataOptions){
			EditorGUILayout.PrefixLabel("Pattern Data");
			script.patternData = (PatternData)EditorGUILayout.ObjectField(script.patternData, typeof(PatternData), false);

			EditorGUILayout.PrefixLabel("Child Prefab");
			script.childrenPrefab = (PatternChildControll)EditorGUILayout.ObjectField(script.childrenPrefab, typeof(PatternChildControll), false);
		
			EditorGUILayout.PrefixLabel("Line Renderer Material");
			script.lineMaterial = (Material)EditorGUILayout.ObjectField(script.lineMaterial, typeof(Material), false);
		}

		// pattern matrix
		GUILayout.Label("Change Pattern", EditorStyles.boldLabel);

		for (int i = 0; i < selectedCoords.Length; i++){
			selectedCoords[i] = Constants.VERTICAL_POSITIONS - script.pattern.coords[i] -1;
		}

		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		Undo.RecordObject(target, "Change Pattern");

		for (int x = 0; x < selectedCoords.Length; x++){

			string[] selectionGridStrings = new string[Constants.VERTICAL_POSITIONS + 1];

			for (int y = 0; y < selectionGridStrings.Length; y++){
				if (y == 0){
					selectionGridStrings[selectionGridStrings.Length - 1 - y] = "none";
				} else {
					selectionGridStrings[selectionGridStrings.Length - 1 - y] = x + ", " + (y - 1);
				}
			}
				
			selectedCoords[x] = Constants.VERTICAL_POSITIONS -
				GUILayout.SelectionGrid(
					selectedCoords[x],
					selectionGridStrings,
					1);

		}

		if (EditorGUI.EndChangeCheck()){
			//script.UpdatePattern();
			int[] coords = new int[Constants.NUMBER_OF_PLAYERS];

			for (int i = 0; i < coords.Length; i++){
				coords[i] = selectedCoords[i] - 1;
			}
			script.ChangePattern(new Pattern(coords));
			EditorUtility.SetDirty(target);
			if (!Application.isPlaying){
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}
		EditorGUILayout.EndHorizontal();

		// random pattern
		randomOptions =  EditorGUILayout.Foldout(randomOptions, "Randomize");
		if (randomOptions){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Symmetrical Pattern");
			symmetrical = EditorGUILayout.Toggle(symmetrical);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Size");
			patternSize = EditorGUILayout.IntField(patternSize);
			patternSize = Mathf.Clamp(patternSize, 2, Constants.NUMBER_OF_PLAYERS);
			if (patternSize % 2 != 0){
				symmetrical = false;
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("Random Pattern")){
				script.ChangePattern(PatternManager.GetRandomPattern(symmetrical, patternSize));
			}
		}

		// audio assignment
		GUILayout.Label("Change Audio Target", EditorStyles.boldLabel);

		EditorGUILayout.BeginVertical();

		EditorGUILayout.BeginHorizontal();

			//EditorGUILayout.PrefixLabel("Category");
			string[] selectedCategoryStrings = {"Rhythm", "Harmony", "Melody"};

			selectedCategory = script.pattern.audioCategory;

			EditorGUI.BeginChangeCheck();
			Undo.RecordObject(target, "Change Category");
			selectedCategory = GUILayout.SelectionGrid(
								selectedCategory,
								selectedCategoryStrings,
								selectedCategoryStrings.Length);
			
			if (EditorGUI.EndChangeCheck()){
				script.pattern.audioCategory = selectedCategory;
				script.UpdatePattern();
				EditorUtility.SetDirty(target);
				if (!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}
		
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();

			//EditorGUILayout.PrefixLabel("Instrument");
			string[] selectedInstrumentStrings = {"Instrument 1", "Instrument 2", "Instrument 3", "Instrument 4"};

			selectedInstrument = script.pattern.instrumentGroup;

			EditorGUI.BeginChangeCheck();
			Undo.RecordObject(target, "Change Instrument");
			selectedInstrument = GUILayout.SelectionGrid(
				selectedInstrument,
				selectedInstrumentStrings,
				selectedInstrumentStrings.Length);

			if (EditorGUI.EndChangeCheck()){
				script.pattern.instrumentGroup = selectedInstrument;
				script.UpdatePattern();
				EditorUtility.SetDirty(target);
				if (!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();

			//EditorGUILayout.PrefixLabel("Variation");
			string[] selectedVariationStrings = {"Variation A", "Variation B", "Variation C", "Variation D"};

			selectedVariation = script.pattern.variation;

			EditorGUI.BeginChangeCheck();
			Undo.RecordObject(target, "Change Variation");
			selectedVariation = GUILayout.SelectionGrid(
				selectedVariation,
				selectedVariationStrings,
				selectedVariationStrings.Length);

			if (EditorGUI.EndChangeCheck()){
				script.pattern.variation = selectedVariation;
				script.UpdatePattern();
				EditorUtility.SetDirty(target);
				if (!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

		
		}
	}


	#region logic functions



	#endregion
}
