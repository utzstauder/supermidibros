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

	public int melodyEveryXBars = 8;
	public int harmonyEveryXBars = 4;
	public int rhythmEveryXBars = 1;

	#endregion


	#region private variables

	// references
	private AudioManager audioManager;
	private Transform dynamicObjects;

	// object pooling lists
	List<PatternControll> pooledPatterns;
	List<Pattern> patternList;

	// spawn preparation
	Dictionary<int, List<PatternControll>> patternsInSceneDict;
	private int prepareXBarsAhead = 8;

	private bool ready = false;

	#endregion


	#region monobehaviour functions

	void Awake(){
		ready = false;

		audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {
			// subscribe to events
			audioManager.OnBar	+= OnBar;
			audioManager.OnBeat += OnBeat;
			audioManager.OnReset += OnReset;
		}

		dynamicObjects = GameObject.Find("DynamicObjects/Patterns").transform;
		if (dynamicObjects == null){
			dynamicObjects = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity) as Transform;
			dynamicObjects.gameObject.name = "DynamicObjects";
		}

		patternsInSceneDict = new Dictionary<int, List<PatternControll>>();

		Pool();
	}

	void Start(){
		// initially prepare the first pattern(s)

		for (int i = firstPatternAt; i < prepareXBarsAhead; i++){
			PreparePatternsForBar(i);
		}

		ready = true;
	}

	void OnEnable(){
		StartCoroutine("PreparePatternsCoroutine");
	}

	void OnDisable(){
		StopCoroutine("PreparePatternsCoroutine");
	}

	#endregion


	#region init functions

	void Pool(){
		// fill lists with patternControll objects
		pooledPatterns = new List<PatternControll>();

		for (int i = 0; i < patternsInSceneLength; i++){
			PatternControll patternControll = Instantiate(patternObjectPrefab, transform.position, Quaternion.identity) as PatternControll;
			patternControll.transform.parent = dynamicObjects;
			patternControll.gameObject.SetActive(false);
			patternControll.SetPrepared(false);
			pooledPatterns.Add(patternControll);
		}
	}

	void OnReset(){
		Reset();
	}

	void Reset(){
		for (int i = 0; i < pooledPatterns.Count; i++){
			pooledPatterns[i].SetPrepared(false);
			pooledPatterns[i].gameObject.SetActive(false);
		}
		patternsInSceneDict.Clear();
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

	private List<PatternControll> GetElementsFromList(int count){
		List<PatternControll> listOfPatternControll = new List<PatternControll>();

		for (int i = 0; i < pooledPatterns.Count; i++){
			if (!pooledPatterns[i].IsPrepared() && pooledPatterns[i].gameObject.activeSelf == false){
				listOfPatternControll.Add(pooledPatterns[i]);
				pooledPatterns.RemoveAt(i);

				if (listOfPatternControll.Count >= count){
					return listOfPatternControll;
				}
			}
		}

		return null;
	}

	#endregion


	#region event handler

	private void OnBar(int bar){
//		if (bar % spawnEveryXBar == 0){
//			SpawnPatternAtBar(bar + spawnAheadOffset);
//			PrepareNextPattern();
//		}

		//Debug.Log(bar);

//		if (patternsInSceneDict.ContainsKey(bar + spawnXBarsAhead)){
//			SpawnPatternAtBar(bar + spawnXBarsAhead);
//		}
	}

	private void OnBeat(int beat){
		// spawn on offbeat
		if (beat > 2){
			if (patternsInSceneDict.ContainsKey(audioManager.GetCurrentBar() + spawnXBarsAhead)){
				SpawnPatternAtBar(audioManager.GetCurrentBar() + spawnXBarsAhead);
			}
		}
	}

	#endregion


	#region spawning functions

	private void SpawnPatternAtBar(int bar){
		for (int i = 0; i < patternsInSceneDict[bar].Count; i++){
			patternsInSceneDict[bar][i].gameObject.SetActive(true);
		}
	}

	#endregion


	#region coroutines

	private IEnumerator PreparePatternsCoroutine(){
		int currentBar, targetBar;

		while (!ready){
			yield return null;
		}

		while (true){
			currentBar = audioManager.GetCurrentBar();
			targetBar = currentBar + prepareXBarsAhead;

			PreparePatternsForBar(targetBar);

			// delete old pattern(s)
			DeleteAtBar(currentBar - 2);

			yield return null;
		}
	}

	#endregion


	#region pattern functions

	void PreparePatternsForBar(int targetBar){
		// prepare new patterns
		if (!patternsInSceneDict.ContainsKey(targetBar)){
			int count = 0;

//			if ((targetBar-1) % harmonyEveryXBars == 0){
//				count = (allZeroes) ? 1 : Random.Range(1, 4);
//			} else if ((targetBar-1) % melodyEveryXBars == 0){
//				count = (allZeroes) ? 1 : Random.Range(1, 3);
//			} else if ((targetBar-1) % rhythmEveryXBars == 0){
//				count = 1;
//			}

			if ((targetBar-1) % rhythmEveryXBars == 0){
				count++;
			}
			if ((targetBar-1) % harmonyEveryXBars == 0){
				count++;	
			}
			if ((targetBar-1) % melodyEveryXBars == 0){
				count++;
			}

			if (allZeroes && count > 0){
				count = 1;
			}

			// get available objects from pool
			List<PatternControll> patternControllList = GetElementsFromList(count);

			if (patternControllList != null){
				// prepare object in scene
				Pattern[] patternList = GetRandomPatterns(count);

				if (allZeroes){
					patternList[0] = Pattern.bottom;
				} 

				AssignAudioToPatterns(targetBar, patternList);

				for (int i = 0; i < count; i++){

					patternControllList[i].MoveToPosition(targetBar, 1, 1);
					patternControllList[i].ChangePattern(patternList[i]);
					patternControllList[i].SetPrepared(true);
				}

				// add object to dict to "reserve" that bar
				patternsInSceneDict.Add(targetBar, patternControllList);
			}
		}
	}

	Pattern GetRandomPatternWithAudio(int targetBar){
		Pattern pattern = GetRandomPattern(symmetricalOnly, GetPatternSize());
		if (allZeroes){
			pattern = Pattern.bottom;
		}

		pattern.audioCategory = audioManager.activeSoundSet.GetRandomIndex();
		pattern.instrumentGroup = audioManager.activeSoundSet.m_audioCategories[pattern.audioCategory].GetRandomIndex(); 
		pattern.variation = audioManager.activeSoundSet.m_audioCategories[pattern.audioCategory].m_audioChannelGroups[pattern.instrumentGroup].GetRandomIndex();


		return pattern;
	}


	Pattern[] GetRandomPatterns(int count){
		count = Mathf.Clamp(count, 1, Constants.VERTICAL_POSITIONS);

		List<Pattern> listOfPatterns = new List<Pattern>();

		for (int i = 0; i < count; i++){
			// prepare new pattern
			Pattern pattern = GetRandomPattern(symmetricalOnly, GetPatternSize());

			listOfPatterns.Add(pattern);

			if (i > 0 && Pattern.Overlap(listOfPatterns)){
				listOfPatterns.Remove(pattern);
				i--;
			}
		}

		return listOfPatterns.ToArray();
	}


	void AssignAudioToPatterns(int targetBar, Pattern[] patterns){
		//Debug.Log(targetBar + " " + patterns.Count);

		if (patterns == null){
			return;
		} else 
			if (patterns.Length == 0){
				return;
			}

//		int numberOfCategoriesToAssign = 1;
		int startIndex = 0;

		if ((targetBar-1) % harmonyEveryXBars == 0){
//			numberOfCategoriesToAssign++;
			startIndex = 1;

		}
		if ((targetBar-1) % melodyEveryXBars == 0){
//			numberOfCategoriesToAssign++;
			startIndex = 2;

		}
			
		//Debug.Log(numberOfCategoriesToAssign + " " + startIndex);

//		foreach(Pattern pattern in patterns){
//			pattern.audioCategory = (startIndex % numberOfCategoriesToAssign);
//			if (allZeroes) pattern.audioCategory = startIndex;
//			pattern.instrumentGroup = audioManager.GetRandomInstrument(pattern.audioCategory);
//			pattern.variation = audioManager.GetRandomVariation(pattern.audioCategory, pattern.instrumentGroup);
//
//			startIndex++;
//			//Debug.Log(pattern.audioCategory + " " + pattern.instrumentGroup + " " + pattern.variation);
//		}

		for (int i = 0; i < patterns.Length; i++){
			patterns[i].audioCategory = ((i + startIndex) % Constants.AUDIO_CATEGORIES);
			if (allZeroes) patterns[i].audioCategory = startIndex;
			patterns[i].instrumentGroup = audioManager.GetRandomInstrument(patterns[i].audioCategory);
			patterns[i].variation = audioManager.GetRandomVariation(patterns[i].audioCategory, patterns[i].instrumentGroup);
		}

	}


	int GetPatternSize(){
		int size = 8;
		if (symmetricalOnly){
			size = Random.Range(1, Constants.NUMBER_OF_PLAYERS/2 + 1) * 2;
		} else {
			size = Random.Range(2, Constants.NUMBER_OF_PLAYERS + 1);
		}
		return size;
	}

	void DeleteAtBar(int bar){
		if (patternsInSceneDict.ContainsKey(bar)){
			//Debug.Log("removing at " + bar);
			foreach (PatternControll patternControll in patternsInSceneDict[bar]){
				patternControll.SetPrepared(false);
				patternControll.gameObject.SetActive(false);
				pooledPatterns.Add(patternControll);
			}
			patternsInSceneDict[bar].Clear();
			patternsInSceneDict.Remove(bar);
		}
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
