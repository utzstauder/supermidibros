using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvironmentManager : MonoBehaviour {

	public EnvironmentSet environmentSet;

	private Dictionary<EnvironmentSet.EnvironmentTile, List<GameObject>> pooledEnvironmentTiles;
	private Dictionary<int, EnvironmentSet.EnvironmentTile> environmentTilesInScene;
	private Dictionary<int, GameObject> environmentTileObjectsInScene;

	private const int barsPerTile = 8;
	private const int spawnTilesInAdvance = 4;

	private Vector3 rotationOffset = new Vector3(0, 0, 0);

	private AudioManager audioManager;
	private Transform dynamicObjects;

	void Awake () {
		if (environmentSet == null){
			Debug.LogError("No EnvironmentSet attached");
		}

		audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {
			audioManager.OnBar += OnBar;
		}

		dynamicObjects = GameObject.Find("DynamicObjects").transform;
		if (dynamicObjects == null){
			dynamicObjects = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity) as Transform;
			dynamicObjects.gameObject.name = "DynamicObjects";
		}

		Init();
	}

	void Start(){
		
	}
	
	void Update () {
	
	}


	#region init functions

	void Init(){
		pooledEnvironmentTiles = new Dictionary<EnvironmentSet.EnvironmentTile, List<GameObject>>();
		environmentTileObjectsInScene = new Dictionary<int, GameObject>();
		environmentTilesInScene = new Dictionary<int, EnvironmentSet.EnvironmentTile>();

		PoolTiles();

		//TODO: spawn first X elements
		for (int i = 0; i < spawnTilesInAdvance; i++){
			PrepareEnvironmentTileAtBar((i * barsPerTile) + 1 + barsPerTile);
			EnableEnvironmentTileInScene((i * barsPerTile) + 1 + barsPerTile);
		}
	}

	void PoolTiles(){
		for (int i = 0; i < environmentSet.environmentTiles.Length; i++){
			int tilesToSpawn = environmentSet.ConnectsToSelf(environmentSet.environmentTiles[i]) ? spawnTilesInAdvance : 1;
			for (int t = 0; t < tilesToSpawn; t++){
				SpawnEnvironmentTile(environmentSet.environmentTiles[i]);
			}
		}
	}

	#endregion


	#region spawn functions

	void OnBar(int bar){
		if ((bar - 1) % barsPerTile == 0){
			EnableEnvironmentTileInScene(bar + (spawnTilesInAdvance * barsPerTile));
			PrepareEnvironmentTileAtBar(bar + (spawnTilesInAdvance * barsPerTile) + barsPerTile);
		}

		if (environmentTileObjectsInScene.ContainsKey(bar - barsPerTile)){
			environmentTileObjectsInScene[bar - barsPerTile].SetActive(false);
			environmentTileObjectsInScene.Remove(bar - barsPerTile);
		}


	}

	void PrepareEnvironmentTileAtBar(int bar){
		EnvironmentSet.EnvironmentTile nextTile;
		if (environmentTilesInScene.ContainsKey(bar - barsPerTile)){
			int index = environmentTilesInScene[bar - barsPerTile].GetNextTileRandom();
			nextTile = environmentSet.environmentTiles[index];
		} else {
			nextTile = environmentSet.environmentTiles[0];
		}

		GameObject tileObject = GetTileFromList(nextTile);

		SnapToGrid snapToGrid = tileObject.GetComponent<SnapToGrid>();
		snapToGrid.m_snapX = true;
		snapToGrid.m_bar = bar;
		snapToGrid.m_snapY = false;
		snapToGrid.m_snapZ = false;
		snapToGrid.UpdatePosition();

		environmentTileObjectsInScene.Add(bar, tileObject);
		environmentTilesInScene.Add(bar, nextTile);
	}

	void EnableEnvironmentTileInScene(int bar){
		if (environmentTileObjectsInScene.ContainsKey(bar)){
			environmentTileObjectsInScene[bar].SetActive(true);
		}
	}

	void SpawnEnvironmentTile(EnvironmentSet.EnvironmentTile tile){
		GameObject tileObject = Instantiate(tile.prefab, transform.position, Quaternion.Euler(rotationOffset)) as GameObject;
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
		List<GameObject> listOfTileObjects = pooledEnvironmentTiles[tile];
		for (int i = 0; i < listOfTileObjects.Count; i++){
			if (listOfTileObjects[i].activeSelf == false){
				return listOfTileObjects[i];
			}
		}

		// if we didn't find an object, we instantiate another and start again
		// TODO: this can be optimised
		SpawnEnvironmentTile(tile);
		return GetTileFromList(tile);
	}

	#endregion


	#region coroutines

	#endregion
}
