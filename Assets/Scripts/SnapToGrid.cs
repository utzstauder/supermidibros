#if UNITY_EDITOR

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SnapToGrid : MonoBehaviour {

	[Header("X")]
	public int 	m_bar 		= 1;
	public int 	m_beat		= 0;
	public int 	m_subBeat 	= 0;

	[Header("Y")]
	[Range(0,7)]
	public int m_verticalPosition = 0;

	[Header("Z")]
	[Range(0,7)]
	public int m_playerLane = 0;

	[Header("Options")]
	public bool	m_snapInPlayMode = false;

	private AudioManager 	m_audioManager;
	private FaderGroup		m_faderGroup;
	private bool 			m_inEditMode 	= true;

	private float targetX = 0;
	private float targetY = 0;
	private float targetZ = 0;

	private Color m_gizmoColor = Color.white;


	// Use this for initialization
	void Awake () {
		if (Application.isPlaying){
			m_inEditMode = false;
		} else {
			m_inEditMode = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		// we only snap in play mode if requested
		if (m_inEditMode || m_snapInPlayMode){
			// get references
			if (!m_audioManager){
				m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
				if (!m_audioManager){
					Debug.LogError("No AudioManager found in this scene!");
				}
			}
			if (!m_faderGroup){
				m_faderGroup = GameObject.Find("FaderGroup").GetComponent<FaderGroup>();
				if (!m_faderGroup){
					Debug.LogError("No FaderGroup found in this scene!");
				}
			}

			// clamp input boundaries
			m_bar = Mathf.Clamp(m_bar, 1, m_audioManager.GetTotalBars());

//			if (m_bar == m_audioManager.GetTotalBars()){
//				m_beat = 0;
//			} else {
				m_beat = Mathf.Clamp(m_beat, 1, m_audioManager.GetTimeSignatureUpper());
//			}

//			if (m_beat == m_audioManager.GetTimeSignatureUpper()){
//				m_subBeat = 0;
//			} else {
				m_subBeat = Mathf.Clamp(m_subBeat, 1, m_audioManager.GetUnitsPerBeat());
//			}

			// calculate target position
			targetX = ((m_bar - 1) * m_audioManager.GetUnitsPerBeat() * m_audioManager.GetTimeSignatureUpper())
				+ ((m_beat - 1) * m_audioManager.GetUnitsPerBeat())
				+ (m_subBeat - 1);

			targetY = (m_faderGroup.m_faderHeight / 8 * m_verticalPosition) + (m_faderGroup.m_faderHeight / 16) + m_faderGroup.m_faderOffset;

			targetZ = m_faderGroup.GetLocalPositionOfFader(m_playerLane).z;

			// set position
			transform.position = new Vector3(targetX, targetY, targetZ);
		}
	}

	void OnDrawGizmosSelected(){
		if (m_inEditMode || m_snapInPlayMode){
			Gizmos.color = m_gizmoColor;

			// vertical lines
			for (int y = 0; y < Constants.NUMBER_OF_PLAYERS; y++){
				
				Vector3 min = new Vector3(	transform.position.x,
											m_faderGroup.m_faderOffset,
											((y - Constants.NUMBER_OF_PLAYERS/2) * m_faderGroup.m_faderPadding + (float)m_faderGroup.m_faderPadding/2));

				Vector3 max = min + Vector3.up * m_faderGroup.m_faderHeight;

				Gizmos.DrawLine(min, max);
			}

			// horizontal lines
			for (int z = 0; z < Constants.NUMBER_OF_PLAYERS; z++){

				Vector3 min = new Vector3(	transform.position.x,
					(z * m_faderGroup.m_faderHeight / Constants.NUMBER_OF_PLAYERS) + (m_faderGroup.m_faderHeight / Constants.NUMBER_OF_PLAYERS)/2,
					m_faderGroup.m_faderPadding * Constants.NUMBER_OF_PLAYERS / 2);

				Vector3 max = min + Vector3.back * m_faderGroup.m_faderPadding * Constants.NUMBER_OF_PLAYERS;

				Gizmos.DrawLine(min, max);
			}
		}
	}
}

#endif