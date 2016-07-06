using UnityEngine;
using System.Collections;
using CustomDataTypes;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(SnapToGrid))]
public class PatternControll : Trigger {

	public PatternChildControll childrenPrefab;

	private bool isPrepared = false;

	private Pattern pattern = Pattern.bottom;

	private PatternChildControll[] children = new PatternChildControll[Constants.NUMBER_OF_PLAYERS];

	private BoxCollider collider;
	private SnapToGrid snapToGrid;

	protected override void Awake () {
		base.Awake();

		collider = GetComponent<BoxCollider>();

		snapToGrid = GetComponent<SnapToGrid>();
		snapToGrid.m_snapY = false;
		snapToGrid.m_snapZ = false;


		for (int i = 0; i < children.Length; i++){
			children[i] = Instantiate(childrenPrefab, transform.position, Quaternion.identity) as PatternChildControll;
			children[i].transform.parent = transform;
			children[i].SetLocalPositionInGrid(i, pattern.coords[i]);
		}
	}

	public void ChangePattern(Pattern _pattern, Color color){
		pattern = _pattern;
		SetActiveStateOfChildren();
		SetPositionOfChildren();
		SetMeshOfChildren();
		SetColorOfChildren(color);

	}

	public void MoveToPosition(int bar, int beat, int subbeat){
		snapToGrid.m_bar		= bar;
		snapToGrid.m_beat		= beat;
		snapToGrid.m_subBeat	= subbeat;

		snapToGrid.m_snapX = true;
		snapToGrid.UpdatePosition();
	}
		
	public bool IsPrepared(){
		return isPrepared;
	}

	public void SetPrepared(bool value){
		isPrepared = value;
	}

	#region player collision

	public void CollisionCheck(int[] playerCoordinates){
		int hits = 0;

		for (int i = 0; i < playerCoordinates.Length; i++){
			if (pattern.coords[i] == playerCoordinates[i]){
				hits++;
			}	
		}

		if (hits == pattern.size){
			OnSuccess();
		} else if(hits > 0){
			OnFailure();
		}
	}

	void DisableThis(){
		gameObject.SetActive(false);
	}

	void OnSuccess(){
		BroadcastTriggerSuccess();
		m_audioManager.OnAudioTrigger(true, pattern.audioCategory, pattern.instrumentGroup, pattern.variation);
	}

	void OnFailure(){
		BroadcastTriggerFailure();
		m_audioManager.OnAudioTrigger(false, pattern.audioCategory, pattern.instrumentGroup, pattern.variation);
	}

	#endregion


	#region child functions

	private void SetActiveStateOfChildren(){
		for (int i = 0; i < children.Length; i++){
			children[i].gameObject.SetActive(pattern.coords[i] >= 0);
		}
	}

	private void SetPositionOfChildren(){
		for (int i = 0; i < children.Length; i++){
			children[i].SetLocalPositionInGrid(i, pattern.coords[i]);
		}
	}

	private void SetMeshOfChildren(){
		for (int i = 0; i < children.Length; i++){
			children[i].SetActiveMeshObject(pattern.audioCategory);
		}
	}

	private void SetColorOfChildren(Color color){
		for (int i = 0; i < children.Length; i++){
			children[i].SetMeshColor(color);
		}
	}

	#endregion
}
