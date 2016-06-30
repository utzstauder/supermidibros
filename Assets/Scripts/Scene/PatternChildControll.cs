using UnityEngine;
using System.Collections;

public class PatternChildControll : MonoBehaviour {

	public GameObject[] audioCategoryObjects = new GameObject[Constants.AUDIO_CATEGORIES];
	private Renderer[] rendererInChildren;

	void Awake(){
		rendererInChildren = GetComponentsInChildren<Renderer>();
	}

	public void SetLocalPositionInGrid(int horizontal, int vertical){
		transform.localPosition = SnapToGrid.GridToWorldCoord(0, vertical, horizontal);
	}

	public void SetActiveMeshObject(int index){
		for (int i = 0; i < audioCategoryObjects.Length; i++){
			audioCategoryObjects[i].SetActive(i == index);
		}
	}

	public void SetMeshColor(Color color){
		for (int i = 0; i < rendererInChildren.Length; i++){
			rendererInChildren[i].material.color = color;
		}
	}
}
