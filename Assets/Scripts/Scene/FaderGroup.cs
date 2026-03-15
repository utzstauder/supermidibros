using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class FaderGroup : MonoBehaviour {

	//[Header("Fader Array")]
	//[Range(1,32)]
	private int m_faderHeight;
	//[Range(0,4)]
	private int m_faderOffset;
	//[Range(1,4)]
	private int m_faderPadding;


	[Header("Collision Detection")]
	[Range(0.01f, 1.0f)]
	public float m_detectionRange = .5f;
	private Collider[] m_collider;
	private Vector3[,] m_gridAsWorldCoords;
	private int[] m_faderPositionsOnGrid;

	[Header("Alignment")]
	public LayerMask alignmentLayerMask;
	public float lookAheadDistance = 32.0f;
	private RaycastHit hitInfo;
	private Collider[] collidersAhead;
	private bool[] isAllignedWithPattern = new bool[Constants.NUMBER_OF_PLAYERS];

	[Header("Gizmos")]
	public Color m_faderColor = Color.blue;


	private Transform[] m_faderGroup = new Transform[Constants.NUMBER_OF_PLAYERS];
	private AudioManager m_audioManager;
	private bool m_inEditMode = true;

	private Dictionary<int, List<Trigger>> triggerReactivationDict;

	void Awake () {
		if (Application.isPlaying){
			m_inEditMode = false;
		} else {
			m_inEditMode = true;
		}

		m_faderHeight = Constants.FADER_HEIGHT;
		m_faderOffset = Constants.FADER_OFFSET;
		m_faderPadding = Constants.FADER_PADDING;

		m_faderGroup = new Transform[transform.childCount];
		for (int i = 0; i < transform.childCount; i++){
			m_faderGroup[i] = transform.GetChild(i);
			m_faderGroup[i].gameObject.name = "Fader_" + i;
		}

		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (m_audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {
			m_audioManager.OnSubBeat += OnSubBeat;
		}

		m_gridAsWorldCoords = SnapToGrid.GridAsWorldCoords();
		m_faderPositionsOnGrid = GetFaderPositionsOnGrid();

		triggerReactivationDict = new Dictionary<int, List<Trigger>>();

	}


	void Update () {
		if (!m_inEditMode){
			// TODO: this can be optimized, listen for *changes* in player input
			// input
			for (int i = 0; i < m_faderGroup.Length; i++){
				m_faderGroup[i].localPosition = Vector3.up * m_faderHeight * MIDIInputManager.instance.GetInputOfPlayer(i)
					+ Vector3.up * m_faderOffset
					+ Vector3.back * ((i - Constants.NUMBER_OF_PLAYERS/2) * m_faderPadding + (float)m_faderPadding/2);
			}

			UpdateAlignmentInfo();

		} else {
			for (int i = 0; i < m_faderGroup.Length; i++){
				m_faderGroup[i].localPosition = Vector3.zero
					+ Vector3.up * m_faderOffset
					+ Vector3.back * ((i - Constants.NUMBER_OF_PLAYERS/2) * m_faderPadding + (float)m_faderPadding/2);
			}
		}
	}


	#region private functions

	void UpdateAlignmentInfo(){
		
		// cast ray
		if (Physics.Raycast(transform.position, Vector3.right, out hitInfo, lookAheadDistance, alignmentLayerMask)){
//			Debug.Log("Hit at " + hitInfo.point);
			collidersAhead = Physics.OverlapSphere(hitInfo.point, m_detectionRange);

			// update positions
			m_faderPositionsOnGrid = GetFaderPositionsOnGrid();

			for (int i = 0; i < Constants.NUMBER_OF_PLAYERS; i++){
				isAllignedWithPattern[i] = false;
			}

			for (int c = 0; c < collidersAhead.Length; c++){
				PatternControll patternControll;
				if(patternControll = collidersAhead[c].GetComponent<PatternControll>()){
					for (int i = 0; i < Constants.NUMBER_OF_PLAYERS; i++){
						if (patternControll.pattern.coords[i] == m_faderPositionsOnGrid[i]){
							isAllignedWithPattern[i] = true;
						}
					}
				}
			}


		}
	}

	/**
	 * This function will check for collision and is called on every subbeat
	 */
	void CheckForCollision(int subBeat){

		// update positions
		m_faderPositionsOnGrid = GetFaderPositionsOnGrid();

		m_collider = Physics.OverlapSphere(transform.position, m_detectionRange);
		for (int c = 0; c < m_collider.Length; c++){
			PatternControll patternControll;
			if(patternControll = m_collider[c].GetComponent<PatternControll>()){
				patternControll.CollisionCheck(m_faderPositionsOnGrid);
			}

			TriggerSingle trigger;
			if(trigger = m_collider[c].GetComponent<TriggerSingle>()){
				trigger.OnCollision(0);
				trigger.gameObject.SetActive(false);
				AddTriggerToReactivationDict(trigger);
			}
		}

	}

	void OnSubBeat(int subBeat){
		CheckForCollision(subBeat);
		ReactivateTriggersInDict(m_audioManager.GetCurrentBar() - 2);
	}

	void AddTriggerToReactivationDict(Trigger trigger){
		int bar = m_audioManager.GetCurrentBar();

		if (triggerReactivationDict.ContainsKey(bar)){
			triggerReactivationDict[bar].Add(trigger);
		} else {
			List<Trigger> triggerList = new List<Trigger>();
			triggerList.Add(trigger);
			triggerReactivationDict.Add(bar, triggerList);
		}
	}

	void ReactivateTriggersInDict(int bar){
		if (triggerReactivationDict.ContainsKey(bar)){
			for (int i = 0; i < triggerReactivationDict[bar].Count; i++){
				triggerReactivationDict[bar][i].gameObject.SetActive(true);
			}
			triggerReactivationDict[bar].Clear();
			triggerReactivationDict.Remove(bar);
		}
	}

	/**
	 * Returns the closest position on the grid of the player
	 */
	int GetClosestVerticalPosition(int playerId){
		int closestY = 0;

		for (int y = 0; y < m_gridAsWorldCoords.GetLength(1); y++){
			Vector3 playerPositionsWithoutX = new Vector3(0, m_faderGroup[playerId].position.y, m_faderGroup[playerId].position.z);
			if (Vector3.Distance(playerPositionsWithoutX, m_gridAsWorldCoords[playerId, y]) < Vector3.Distance(playerPositionsWithoutX, m_gridAsWorldCoords[playerId, closestY])){
				closestY = y;
			}
		}

		return closestY;
	}

	int[] GetFaderPositionsOnGrid(){
		int[] positions = new int[Constants.NUMBER_OF_PLAYERS];
		for (int i = 0; i < positions.Length; i++){
			positions[i] = GetClosestVerticalPosition(i);
		}

		return positions;
	}

	#endregion


	#region public getter

	public bool[] GetAlignmentInfo(){
		return isAllignedWithPattern;
	}

	public Vector3 GetLocalPositionOfFader(int _faderId){
		if (m_faderGroup.Length > 0){
			if (m_faderGroup[_faderId] != null){
				return m_faderGroup[_faderId].localPosition;
			}
		}
		return Vector3.zero;
	}

	public float GetRelativePositionOfFader(int _faderId){
		return MIDIInputManager.instance.GetInputOfPlayer(_faderId);
	}

	#endregion

	void PrintPositions(int beat){
		string positions = "";
		for (int i = 0; i < m_faderGroup.Length; i++){
			positions += GetClosestVerticalPosition(i) + ", ";
		}
		print(positions);
	}

	void OnDrawGizmos(){
		Gizmos.color = m_faderColor;

		for (int i = 0; i < m_faderGroup.Length; i++){
			Vector3 min = new Vector3(	transform.position.x,
										m_faderOffset,
				((i - Constants.NUMBER_OF_PLAYERS/2) * m_faderPadding + (float)m_faderPadding/2));
			
			Vector3 max = min + Vector3.up * m_faderHeight;

			Gizmos.DrawLine(min, max);

			Gizmos.DrawWireSphere(m_faderGroup[i].position, m_detectionRange);
		}
	}
}
