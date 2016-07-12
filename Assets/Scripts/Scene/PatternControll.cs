using UnityEngine;
using System.Collections;
using CustomDataTypes;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(SnapToGrid))]
public class PatternControll : Trigger {

	public PatternData patternData;
	public PatternChildControll childrenPrefab;
	public Material lineMaterial;
	public Color particleColorSuccess = Color.green;
	public Color particleColorFailure = Color.red;

	public float lineWidth = 0.35f;

	private bool isPrepared = false;

	public Pattern pattern = Pattern.bottom;

	private PatternChildControll[] children = new PatternChildControll[Constants.NUMBER_OF_PLAYERS];

	private BoxCollider collider;
	private SnapToGrid snapToGrid;
	private LineRenderer lineRenderer;

	protected override void Awake () {
		base.Awake();

		collider = GetComponent<BoxCollider>();

		snapToGrid = GetComponent<SnapToGrid>();

		Init();
	}

	void Init(){
		snapToGrid.m_snapY = false;
		snapToGrid.m_lockY = true;
		snapToGrid.m_snapZ = false;
		snapToGrid.m_lockZ = true;
		transform.position = Vector3.zero;
		snapToGrid.UpdatePosition();

		// delete old children
		PatternChildControll[] childControls = GetComponentsInChildren<PatternChildControll>();

		foreach(PatternChildControll child in childControls){
			if (Application.isPlaying){ 
				Destroy(child.gameObject);
			} else {
				DestroyImmediate(child.gameObject);
			}
		}

		pattern = Pattern.bottom;

		children = new PatternChildControll[Constants.NUMBER_OF_PLAYERS];

		// spawn new children
		for (int i = 0; i < children.Length; i++){
			children[i] = Instantiate(childrenPrefab, transform.position, Quaternion.identity) as PatternChildControll;
			children[i].transform.parent = transform;
			children[i].SetLocalPositionInGrid(i, pattern.coords[i]);
		}

		// add line renderer
		lineRenderer = gameObject.AddComponent<LineRenderer>();
	}

	public void ChangePattern(Pattern _pattern){
		//Init();

		pattern = _pattern;
		SetActiveStateOfChildren();
		SetPositionOfChildren();
		SetMeshOfChildren();
		SetColorOfChildren();
		SetAffectorsOfChildren(pattern.audioCategory, pattern.instrumentGroup, pattern.variation);
		UpdateLineRenderer();

	}

	public void UpdatePattern(){
		//Init();

		SetActiveStateOfChildren();
		SetPositionOfChildren();
		SetMeshOfChildren();
		SetColorOfChildren();
		SetAffectorsOfChildren(pattern.audioCategory, pattern.instrumentGroup, pattern.variation);
		UpdateLineRenderer();
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
		collider.enabled = true;
	}

	void UpdateLineRenderer(){
		lineRenderer.SetWidth(lineWidth, lineWidth);
		lineRenderer.SetVertexCount(pattern.size);
		lineRenderer.SetPositions(GetPositionsOfChildren());
		Color lineRendererColor = patternData.categoryColors[pattern.audioCategory];
		lineRendererColor.a = 0.5f;
		if (Application.isPlaying){
			lineRenderer.material = lineMaterial;
			lineRenderer.material.color = lineRendererColor;
		} else {
			lineRenderer.sharedMaterial = lineMaterial;
			lineRenderer.sharedMaterial.color = lineRendererColor;
		}
	}

	#region player collision

	public void CollisionCheck(int[] playerCoordinates){
		int hits = 0;
		bool[] hitPositions = new bool[playerCoordinates.Length];

		for (int i = 0; i < playerCoordinates.Length; i++){
			if (pattern.coords[i] == playerCoordinates[i]){
				hits++;
				hitPositions[i] = true;
			}
			else {
				hitPositions[i] = false;
			}
		}

		DisableRendererOfChildren(hitPositions);

		if (hits == pattern.size){
			EmitParticlesSuccess();
			OnSuccess();
		} else if(hits > 0){
			EmitParticlesFailure(hitPositions);
			OnFailure();
		}
			
		collider.enabled = false;
	}

	public bool[] GetCollisionCheckInfo(int[] playerCoordinates){
		bool[] returnValues = new bool[playerCoordinates.Length];

		for (int i = 0; i < playerCoordinates.Length; i++){
			returnValues[i] = (pattern.coords[i] == playerCoordinates[i]);
		}

		return returnValues;
	}

	void DisableThis(){
		gameObject.SetActive(false);
	}

	void OnSuccess(){
		BroadcastTriggerSuccess();
		GameManager.instance.IncreasePatternCombo(pattern.size);
		m_audioManager.OnAudioTrigger(true, pattern.audioCategory, pattern.instrumentGroup, pattern.variation);
	}

	void OnFailure(){
		BroadcastTriggerFailure();
		GameManager.instance.DecreasePatternCombo(pattern.size);
		m_audioManager.OnAudioTrigger(false, pattern.audioCategory, pattern.instrumentGroup, pattern.variation);
	}

	#endregion


	#region child functions

	private void EmitParticlesSuccess(){
		SetParticlesMaterialInChildren(true);
		EmitParticles(particleColorSuccess);
	}

	private void EmitParticlesFailure(bool[] hitPositions){
		SetParticlesMaterialInChildren(false);
		EmitParticles(particleColorFailure, hitPositions);
	}

	private void EmitParticles(Color color){
		for (int i = 0; i < children.Length; i++){
			children[i].EmitParticles(color);
		}
	}

	private void EmitParticles(Color color, bool[] emit){
		for (int i = 0; i < children.Length; i++){
			if (emit[i]){
				children[i].EmitParticles(color);
			}
		}
	}

	private void SetParticlesMaterialInChildren(bool success){
		for (int i = 0; i < children.Length; i++){
			children[i].SetParticlesMaterial(success);
		}
	}

	private void SetGravityScaleInChildren(float value){
		for (int i = 0; i < children.Length; i++){
			children[i].SetParticleGravityScale(value);
		}
	}

	private void SetActiveStateOfChildren(){
		for (int i = 0; i < children.Length; i++){
			children[i].gameObject.SetActive(pattern.coords[i] >= 0);
		}
	}

	private void DisableRendererOfChildren(bool[] active){
		for (int i = 0; i < children.Length; i++){
			if (active[i])
			{
				children[i].SetRendererActive(false);
			}
		}
	}

	private void SetPositionOfChildren(){
		for (int i = 0; i < children.Length; i++){
			children[i].SetLocalPositionInGrid(i, pattern.coords[i]);
		}
	}

	private Vector3[] GetPositionsOfChildren(){
		Vector3[] positions = new Vector3[pattern.size];
		int index = 0;

		for (int i = 0; i < children.Length; i++){
			if (pattern.coords[i] >= 0){
				positions[index] = transform.position + children[i].transform.localPosition;
				index++;
			}
		}
	
		return positions;
	}

	private void SetMeshOfChildren(){
		for (int i = 0; i < children.Length; i++){
			children[i].SetActiveMeshObject(pattern.audioCategory);
		}
	}

	private void SetColorOfChildren(){
		for (int i = 0; i < children.Length; i++){
			children[i].SetMeshColor(patternData.categoryColors[pattern.audioCategory]);
		}
	}

	private void SetAffectorsOfChildren(int category, int instrument, int variation){
		for (int i = 0; i < children.Length; i++){
			OnRhythmPeriodicAffectorAudioChannel affector = children[i].gameObject.GetComponentInChildren<OnRhythmPeriodicAffectorAudioChannel>();

			affector.category = category;
			affector.instrument = -1;
			affector.variation = -1;
		}
	}

	#endregion
}
