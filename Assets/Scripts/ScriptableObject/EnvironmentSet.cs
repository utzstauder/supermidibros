using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using CustomDataTypes;

[CreateAssetMenu (menuName = "Super MIDI Bros./EnvironmentSet", fileName = "EnvironmentSet", order = 100)]
public class EnvironmentSet : ScriptableObject {

	public EnvironmentTile[] environmentTiles;

	[System.Serializable]
	public class EnvironmentTile{
		public string name;
		public GameObject prefab;
		public int[] connectsTo;
		public int length;

		public EnvironmentTile(string _name, GameObject _prefab, params int[] _connectsTo){
			name = _name;
			connectsTo = _connectsTo;
			if (_connectsTo.Length < 1){
				connectsTo = new int[1] {0};
			}
			prefab = _prefab;
			length = 1;
		}

		public int GetNextTileRandom(){
			return connectsTo[Random.Range(0, connectsTo.Length)];
		}

		public GameObject GetPrefab(){
			return prefab;
		}
	}

	// returns true if tile can connect to itself
	public bool ConnectsToSelf(EnvironmentTile tile){
		int indexOfTile = GetIndexOfTile(tile);

		for (int i = 0; i < tile.connectsTo.Length; i++){
			if (tile.connectsTo[i] == indexOfTile){
				return true;
			}
		}

		return false;
	}

	public int GetIndexOfTile(EnvironmentTile tile){
		for (int i = 0; i < environmentTiles.Length; i++){
			if (tile == environmentTiles[i]){
				return i;
			}
		}
		return -1;
	}

}
