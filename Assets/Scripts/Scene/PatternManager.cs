using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomDataTypes;

/**
 *  This class manages procedural pattern spawning 
 */
public class PatternManager : MonoBehaviour {

	#region public variables

	[Header("Pattern Options")]
	public PatternControll patternObjectPrefab;
	public Color[] patternColors;

	private const int patternsInSceneLength = 64;

	[Header("Spawn Options")]
	public bool allZeroes = false;
	public bool symmetricalOnly = false;
	public int firstPatternAt = 2;
	public int spawnXBarsAhead = 2;
	public int spawnEveryXBar = 1;

	#endregion


	#region private variables

	// references
	private AudioManager audioManager;
	private Transform dynamicObjects;

	// object pooling lists
	List<PatternControll> pooledPatterns;
	List<Pattern> patternList;

	// spawn preparation
	PatternControll nextPattern;
	Dictionary<int, PatternControll> patternsInSceneDict;
	private int prepareXBarsAhead = 4;

	#endregion


	#region monobehaviour functions

	void Awake(){
		audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {
			// subscribe to events
			audioManager.OnBar	+= OnBar;
			audioManager.OnBeat += OnBeat;
		}

		dynamicObjects = GameObject.Find("DynamicObjects").transform;
		if (dynamicObjects == null){
			dynamicObjects = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity) as Transform;
			dynamicObjects.gameObject.name = "DynamicObjects";
		}

		patternsInSceneDict = new Dictionary<int, PatternControll>();

		Init();
	}

	void OnEnable(){
		StartCoroutine("PreparePatternsCoroutine");
	}

	void OnDisable(){
		StopCoroutine("PreparePatternsCoroutine");
	}

	#endregion


	#region init functions

	void Init(){
		// fill lists with patternControll objects
		pooledPatterns = new List<PatternControll>();
		for (int i = 0; i < patternsInSceneLength; i++){
			PatternControll patternControll = Instantiate(patternObjectPrefab, transform.position, Quaternion.identity) as PatternControll;
			patternControll.transform.parent = dynamicObjects;
			patternControll.gameObject.SetActive(false);
			pooledPatterns.Add(patternControll);
		}

		// initially prepare the first pattern(s)

		for (int i = firstPatternAt; i < prepareXBarsAhead; i++){
			// get available object from pool
			PatternControll patternControll = GetElementFromList();

			// prepare object in scene
			Pattern pattern = GetRandomPatternWithAudio();
			patternControll.ChangePattern(pattern, patternColors[pattern.audioCategory]);
			patternControll.MoveToPosition(i, 1, 1);

			// add object to dict to "reserve" that bar
			patternControll.SetPrepared(true);
			patternsInSceneDict.Add(i, patternControll);
		}
	}

	#endregion


	#region list handler

	private PatternControll GetElementFromList(){
		for (int i = 0; i < pooledPatterns.Count; i++){
			if (!pooledPatterns[i].IsPrepared()){
				return pooledPatterns[i];
			}
		}
		return null;
	}

	#endregion


	#region event handler

	private void OnBar(int bar){
		// TODO: just a test, spawn patterns every other bar
//		if (bar % spawnEveryXBar == 0){
//			SpawnPatternAtBar(bar + spawnAheadOffset);
//			PrepareNextPattern();
//		}

		if (patternsInSceneDict.ContainsKey(bar + spawnXBarsAhead)){
			SpawnPatternAtBar(bar + spawnXBarsAhead);
		}
	}

	private void OnBeat(int beat){
		
	}

	#endregion


	#region spawning functions

	// DEPRECATED
	// TODO: just for testing; needs rework
	private void PrepareNextPattern(){
		nextPattern = GetElementFromList();

		if (nextPattern == null){
			Debug.Log("There was no available object in the pool. Try extending it.");
		} else {
			int size = 8;
			if (symmetricalOnly){
				size = Random.Range(1, Constants.NUMBER_OF_PLAYERS/2 + 1) * 2;
			} else {
				size = Random.Range(2, Constants.NUMBER_OF_PLAYERS + 1);
			}
			Pattern pattern = GetRandomPattern(true, size);
			if (allZeroes){
				pattern = Pattern.bottom;
			}
			pattern.audioCategory = audioManager.m_soundSet.GetRandomIndex();
			pattern.instrumentGroup = audioManager.m_soundSet.m_audioCategories[pattern.audioCategory].GetRandomIndex(); 
			pattern.variation = audioManager.m_soundSet.m_audioCategories[pattern.audioCategory].m_audioChannelGroups[pattern.instrumentGroup].GetRandomIndex();
			nextPattern.ChangePattern(pattern, patternColors[pattern.audioCategory]);
		}
	}

	private void SpawnPatternAtBar(int bar){
//		if (nextPattern == null){
//			Debug.Log("No nextPattern available.");
//		} else {
//			nextPattern.MoveToPosition(bar, 1, 1);
//			nextPattern.gameObject.SetActive(true);
//		}
		Debug.Log("Spawing pattern at " + bar);
		patternsInSceneDict[bar].gameObject.SetActive(true);
	}

	#endregion


	#region coroutines

	private IEnumerator PreparePatternsCoroutine(){
		int currentBar, targetBar;
		patternsInSceneDict = new Dictionary<int, PatternControll>();

		while (true){
			currentBar = audioManager.GetCurrentBar();
			targetBar = currentBar + prepareXBarsAhead;

			// prepare new patterns
			if (!patternsInSceneDict.ContainsKey(targetBar)){
				Debug.Log("Preparing new pattern for bar " + targetBar);
				// there is no pattern at targetBar, prepare one

				// get available object from pool
				PatternControll patternControll = GetElementFromList();
				while (patternControll == null){
					yield return null;
					patternControll = GetElementFromList();
				}
					
				// prepare object in scene
				Pattern pattern = GetRandomPatternWithAudio();
				patternControll.ChangePattern(pattern, patternColors[pattern.audioCategory]);
				patternControll.MoveToPosition(targetBar, 1, 1);

				// add object to dict to "reserve" that bar
				patternControll.SetPrepared(true);
				patternsInSceneDict.Add(targetBar, patternControll);
			}

			// delete old pattern(s)
			if (patternsInSceneDict.ContainsKey(currentBar - 1)){
				//Debug.Log("disabling pattern at bar " + (currentBar - 1));
				patternsInSceneDict[currentBar - 1].SetPrepared(false);
				patternsInSceneDict[currentBar - 1].gameObject.SetActive(false);
				patternsInSceneDict.Remove(currentBar - 1);
			}

			yield return null;
		}
	}

	#endregion


	#region private helpers

	Pattern GetRandomPatternWithAudio(){
		int size = 8;
		if (symmetricalOnly){
			size = Random.Range(1, Constants.NUMBER_OF_PLAYERS/2 + 1) * 2;
		} else {
			size = Random.Range(2, Constants.NUMBER_OF_PLAYERS + 1);
		}
		Pattern pattern = GetRandomPattern(true, size);
		if (allZeroes){
			pattern = Pattern.bottom;
		}
		pattern.audioCategory = audioManager.m_soundSet.GetRandomIndex();
		pattern.instrumentGroup = audioManager.m_soundSet.m_audioCategories[pattern.audioCategory].GetRandomIndex(); 
		pattern.variation = audioManager.m_soundSet.m_audioCategories[pattern.audioCategory].m_audioChannelGroups[pattern.instrumentGroup].GetRandomIndex();

		return pattern;
	}

	#endregion


	#region static functions

	// TODO: while loop might not be the right solution
	public static Pattern GetRandomPattern(bool symmetrical = false, int size = 8){
		int[] coords = new int[Constants.NUMBER_OF_PLAYERS];
		int currentSize;

		if (size < 2){
			size = 2;
		}

		if (size % 2 != 0){
			symmetrical = false;
		}
			
		do{
			currentSize = 0;

			for (int i = 0; i < Constants.NUMBER_OF_PLAYERS; i++){
			
				int value = (currentSize < size) ? Random.Range(-Constants.VERTICAL_POSITIONS, Constants.VERTICAL_POSITIONS) : -1;
				value = Mathf.Clamp(value, -1, Constants.VERTICAL_POSITIONS - 1);

				coords[i] = value;

				if (coords[i] > -1){
					currentSize += (symmetrical) ? 2 : 1;
				}

				if (symmetrical){
					coords[Constants.NUMBER_OF_PLAYERS - 1 - i] = value;
					if (i >= (Constants.NUMBER_OF_PLAYERS / 2) - 1){
						break;
					}
				}
			}
		} while (currentSize < size);

		return new Pattern(coords);;
	}

	#endregion
}
