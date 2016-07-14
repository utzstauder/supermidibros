using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Super MIDI Bros./PlayerCharacterData", fileName = "PlayerCharacterData")]
public class PlayerCharacterData : ScriptableObject {

	public Gradient playerColorGradient;

	public PlayerCharacter[] playerCharacters;


	#region public functions

	public int GetPlayerCharacterIndexUnlockedAtScore(int score){
		int closestObjectIndex = -1;
		int closestScoreBelow = int.MinValue;

		for (int i = 0; i < playerCharacters.Length; i++){
			if (playerCharacters[i].minScoreToUnlock == score){
				return i;
			}

			if (playerCharacters[i].minScoreToUnlock < score){
				if (playerCharacters[i].minScoreToUnlock > closestScoreBelow){
					closestObjectIndex = i;
					closestScoreBelow = playerCharacters[i].minScoreToUnlock;
				}
			} 
		}

		return closestObjectIndex;

	}

	#endregion


	#region structs

	[System.Serializable]
	public struct PlayerCharacter{
		public string name;
		public GameObject prefab;
		public int minScoreToUnlock;

		public PlayerCharacter(string _name){
			name = _name;
			prefab = null;
			minScoreToUnlock = 0;
		}
	}

	#endregion
}
