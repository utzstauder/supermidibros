using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvironmentManager : MonoBehaviour {

	public EnvironmentSet environmentSet;
	public int startAtIndex = 0;
	public bool randomVariations = false;

	private Dictionary<EnvironmentSet.EnvironmentTile, List<GameObject>> pooledEnvironmentTiles;
	private Dictionary<int, EnvironmentSet.EnvironmentTile> environmentTilesInScene;
	private Dictionary<int, GameObject> environmentTileObjectsInScene;
	private Dictionary<EnvironmentSet.EnvironmentTile, int> variationsLengthDict;
	private Dictionary<EnvironmentSet.EnvironmentTile, int> variationsIndexDict;

	private const int barsPerTile = 8;
	private const int spawnTilesInAdvance = 4;

	private Vector3 rotationOffset = new Vector3(0, 0, 0);

	private AudioManager audioManager;
	private Transform dynamicObjects;

	void Awake () {
		// Always create dicts so other scripts don't get null refs
		pooledEnvironmentTiles = new Dictionary<EnvironmentSet.EnvironmentTile, List<GameObject>>();
		environmentTileObjectsInScene = new Dictionary<int, GameObject>();
		environmentTilesInScene = new Dictionary<int, EnvironmentSet.EnvironmentTile>();
		variationsIndexDict = new Dictionary<EnvironmentSet.EnvironmentTile, int>();
		variationsLengthDict = new Dictionary<EnvironmentSet.EnvironmentTile, int>();

		if (environmentSet == null){
			Debug.LogError("[EnvironmentManager] No EnvironmentSet attached. Assign an EnvironmentSet in the Inspector to restore the environment.");
			SubscribeToAudio();
			return;
		}

		SubscribeToAudio();

		// Ensure hierarchy: DynamicObjects/Environment
		dynamicObjects = GameObject.Find("DynamicObjects/Environment")?.transform;
		if (dynamicObjects == null){
			// Ensure root exists
			var root = GameObject.Find("DynamicObjects")?.transform;
			if (root == null){
				root = new GameObject("DynamicObjects").transform;
				root.position = Vector3.zero;
				root.rotation = Quaternion.identity;
			}
			// Create Environment child under root
			var envGO = new GameObject("Environment");
			envGO.transform.SetParent(root, false);
			dynamicObjects = envGO.transform;
		}

		PoolTiles();
		SetVariationsIndices();
		SpawnFirst();
	}

	void SubscribeToAudio(){
		audioManager = GameObject.Find("AudioManager")?.GetComponent<AudioManager>();
		if (audioManager == null){
			Debug.LogWarning("[EnvironmentManager] No AudioManager found in scene.");
			return;
		}
		audioManager.OnBar += OnBar;
		audioManager.OnBeat += OnBeat;
		audioManager.OnSubBeat += OnSubBeat;
		audioManager.OnStop += OnStop;
		audioManager.OnReset += OnReset;
	}

	#region init functions

	void SpawnFirst(){
		if (environmentSet == null) return;
		for (int i = 0; i < spawnTilesInAdvance; i++){
			int bar = (i * barsPerTile) + 1;
			PrepareEnvironmentTileAtBar(bar);
			EnableEnvironmentTileInScene(bar);
			Debug.Log($"[Environment] SpawnFirst prepared and enabled bar {bar}");
		}
	}

	void PoolTiles(){
		if (environmentSet == null || environmentSet.environmentTiles == null) return;
		for (int i = 0; i < environmentSet.environmentTiles.Length; i++){
			var tile = environmentSet.environmentTiles[i];
			if (tile == null || tile.prefab == null){
				Debug.LogWarning($"[EnvironmentManager] Skipping null tile or prefab at index {i}");
				continue;
			}
			int tilesToSpawn = environmentSet.ConnectsToSelf(tile) ? spawnTilesInAdvance : 1;
			for (int t = 0; t < tilesToSpawn; t++){
				SpawnEnvironmentTile(tile);
			}
		}
	}

	void SetVariationTransforms(){
		
	}

	void SetVariationsIndices(){
		if (environmentSet == null || environmentSet.environmentTiles == null) return;
		for(int i = 0; i < environmentSet.environmentTiles.Length; i++){
			var tile = environmentSet.environmentTiles[i];
			if (tile == null) continue;
			int length = 0;
			if (tile.prefab != null){
				Transform variationsParent = tile.prefab.transform.Find("Variations");
				if (variationsParent != null)
					length = variationsParent.childCount;
			}
			variationsLengthDict.Add(tile, length);
			variationsIndexDict.Add(tile, 0);
		}
	}

	#endregion

	void OnStop(){
		Reset();
	}

	void OnReset(){
		Reset();
	}

	public void Reset(){
		if (environmentTileObjectsInScene == null || environmentTilesInScene == null) return;
		environmentTileObjectsInScene.Clear();
		environmentTilesInScene.Clear();
		if (environmentSet == null) return;
		for (int i = 0; i < spawnTilesInAdvance; i++){
			PrepareEnvironmentTileAtBar((i * barsPerTile) + 1);
			EnableEnvironmentTileInScene((i * barsPerTile) + 1);
		}
	}


	#region spawn functions

	void OnBar(int bar){
		
	}

	void OnBeat(int beat){
		DeleteTileAtBar(audioManager.GetCurrentBar());
	}

	void OnSubBeat(int subbeat){
		SpawnTileAtBar(audioManager.GetCurrentBar());
	}

	void SpawnTileAtBar(int bar){
		if (environmentSet == null) return;
		if ((bar - 1) % barsPerTile == 0){
			EnableEnvironmentTileInScene(bar + (spawnTilesInAdvance * barsPerTile));

			if (!environmentTileObjectsInScene.ContainsKey(bar + ((spawnTilesInAdvance - 1) * barsPerTile) + barsPerTile)){
				PrepareEnvironmentTileAtBar(bar + ((spawnTilesInAdvance - 1) * barsPerTile) + barsPerTile);
			}
		}
	}

	void DeleteTileAtBar(int bar){
		if (environmentTileObjectsInScene == null) return;
		if (environmentTileObjectsInScene.ContainsKey(bar - (barsPerTile * 2))){
			var go = environmentTileObjectsInScene[bar - (barsPerTile * 2)];
			if (go != null) go.SetActive(false);
			environmentTileObjectsInScene.Remove(bar - (barsPerTile * 2));
		}
	}

	void PrepareEnvironmentTileAtBar(int bar){
		if (environmentSet == null || environmentTileObjectsInScene == null) return;
		if (environmentTileObjectsInScene.ContainsKey(bar)){
			return;
		}

		EnvironmentSet.EnvironmentTile nextTile;
		if (environmentTilesInScene.ContainsKey(bar - barsPerTile)){
			int index = environmentTilesInScene[bar - barsPerTile].GetNextTileRandom();
			nextTile = environmentSet.environmentTiles[index];
		} else {
			int start = Mathf.Clamp(startAtIndex, 0, environmentSet.environmentTiles.Length - 1);
			nextTile = environmentSet.environmentTiles[start];
		}

		if (nextTile == null || nextTile.prefab == null){
			Debug.LogWarning($"[EnvironmentManager] PrepareEnvironmentTileAtBar({bar}): nextTile or prefab is null.");
			return;
		}

		GameObject tileObject = GetTileFromList(nextTile);
		if (tileObject == null){
			Debug.LogWarning($"[EnvironmentManager] PrepareEnvironmentTileAtBar({bar}): no tile instance for '{nextTile.name}'.");
			return;
		}

		Transform variationsTransform = tileObject.transform.Find("Variations");

		SnapToGrid snapToGrid = tileObject.GetComponent<SnapToGrid>();
		snapToGrid.m_snapX = true;
		snapToGrid.m_bar = bar;
		snapToGrid.m_snapY = false;
		snapToGrid.m_snapZ = false;
		snapToGrid.UpdatePosition();


		if (variationsTransform != null && variationsLengthDict.TryGetValue(nextTile, out int length) && length > 0
		    && variationsIndexDict.TryGetValue(nextTile, out int currentIndex)){
			if (randomVariations){
				int newIndex = Random.Range(0, length);
				variationsIndexDict[nextTile] = newIndex;
				EnableChild(variationsTransform, newIndex);
			} else {
				EnableChild(variationsTransform, currentIndex);
				variationsIndexDict[nextTile] = (currentIndex + 1) % length;
			}
		} else if (variationsTransform != null){
			Debug.Log(nextTile.name + ": no variations or not in dict");
		}

		environmentTileObjectsInScene.Add(bar, tileObject);
		environmentTilesInScene.Add(bar, nextTile);

		Debug.Log($"[Environment] Prepared bar {bar} with tile '{nextTile.name}' -> object '{tileObject.name}' at x={tileObject.transform.localPosition.x}");
	}

	void EnableChild(Transform parent, int childIndex){
		for (int i = 0; i < parent.childCount; i++){
			parent.GetChild(i).gameObject.SetActive(i == childIndex);
		}
	}

	void EnableEnvironmentTileInScene(int bar){
		if (environmentTileObjectsInScene == null) return;
		if (environmentTileObjectsInScene.ContainsKey(bar)){
			var go = environmentTileObjectsInScene[bar];
			if (go != null){
				go.SetActive(true);
				Debug.Log($"[Environment] Enabled bar {bar} object '{go.name}' active={go.activeSelf}");
			}
		}
	}

	void SpawnEnvironmentTile(EnvironmentSet.EnvironmentTile tile){
		if (tile == null || tile.prefab == null){
			Debug.LogWarning("[EnvironmentManager] SpawnEnvironmentTile: tile or prefab is null.");
			return;
		}
		if (dynamicObjects == null) return;

		GameObject tileObject = Instantiate(tile.prefab, transform.position, Quaternion.Euler(rotationOffset)) as GameObject;
		if (tileObject == null) return;
		tileObject.transform.parent = dynamicObjects;
		tileObject.SetActive(false);

		if (pooledEnvironmentTiles.ContainsKey(tile)){
			pooledEnvironmentTiles[tile].Add(tileObject);
		} else {
			List<GameObject> tileObjectList = new List<GameObject>();
			tileObjectList.Add(tileObject);
			pooledEnvironmentTiles.Add(tile, tileObjectList);
		}
	}

	GameObject GetTileFromList(EnvironmentSet.EnvironmentTile tile){
		if (tile == null || !pooledEnvironmentTiles.TryGetValue(tile, out List<GameObject> listOfTileObjects) || listOfTileObjects == null)
			return null;
		for (int i = 0; i < listOfTileObjects.Count; i++){
			if (listOfTileObjects[i] != null && listOfTileObjects[i].activeSelf == false){
				return listOfTileObjects[i];
			}
		}
		SpawnEnvironmentTile(tile);
		return GetTileFromList(tile);
	}

	#endregion

	public GameObject GetEnvironmentTileGameObjectAtBar(int bar){
		return (environmentTileObjectsInScene.ContainsKey(bar)) ? environmentTileObjectsInScene[bar] : null;
	}

}
