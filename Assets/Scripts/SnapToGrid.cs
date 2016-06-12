#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SnapToGrid : MonoBehaviour {

	[Header("X")]
	public int 	m_bar 		= 1;
	public int 	m_beat		= 0;
	public int 	m_subBeat 	= 0;
	private float m_prevX;

	[Header("Y")]
	[Range(0,7)]
	public int m_verticalPosition = 0;
	private float m_prevY;

	[Header("Z")]
	[Range(0,7)]
	public int m_playerLane = 0;
	private float m_prevZ;

	[Header("Check LockPosition everytime! Bugs, bugs, bugs...")]
	// TODO: lock position on deselect
	public bool m_lockPosition		= false;
	public bool	m_snapInPlayMode	= false;

	private AudioManager 	m_audioManager;
	private FaderGroup		m_faderGroup;
	private bool 			m_inEditMode 	= true;

	private float targetX = 0;
	private float targetY = 0;
	private float targetZ = 0;

	private Color m_gizmoColor = Color.white * 0.5f;


	// Use this for initialization
	void Awake () {
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

		if (Application.isPlaying){
			m_inEditMode = false;
			m_lockPosition = true;
		} else {
			m_inEditMode = true;
		}

		m_prevX = transform.position.x;
		m_prevY = transform.position.y;
		m_prevZ = transform.position.z;
	}
	
	// Update is called once per frame
	void Update () {
		// we only snap in play mode if requested
		if (m_inEditMode || m_snapInPlayMode){
			if (m_lockPosition){
				transform.position = new Vector3(m_prevX, m_prevY, m_prevZ);
			} else {
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


				// calculate new x coordinate

				// snap x position on manual change in scene view
				if (transform.position.x != m_prevX){
					targetX = Mathf.Round(transform.position.x);

					m_bar		= (int)targetX / (m_audioManager.GetUnitsPerBeat() * m_audioManager.GetTimeSignatureUpper()) + 1;
					m_beat		= ((int)targetX / m_audioManager.GetUnitsPerBeat()) % m_audioManager.GetTimeSignatureUpper() + 1;
					m_subBeat	= (int)targetX % m_audioManager.GetUnitsPerBeat() + 1;
				}

				// clamp input boundaries
				m_bar = Mathf.Clamp(m_bar, 1, m_audioManager.GetTotalBars());
				m_beat = Mathf.Clamp(m_beat, 1, m_audioManager.GetTimeSignatureUpper());
				m_subBeat = Mathf.Clamp(m_subBeat, 1, m_audioManager.GetUnitsPerBeat());

				targetX = ((m_bar - 1) * m_audioManager.GetUnitsPerBeat() * m_audioManager.GetTimeSignatureUpper())
					+ ((m_beat - 1) * m_audioManager.GetUnitsPerBeat())
					+ (m_subBeat - 1);


				// calculate new y coordinate

				// snap vertical position on manual change in scene view
				if (transform.position.y != m_prevY){
					m_verticalPosition = Mathf.RoundToInt((7 * (transform.position.y /*- ((float)m_faderGroup.m_faderHeight / 16.0f)*/ + (float)m_faderGroup.m_faderOffset) /
						(float)m_faderGroup.m_faderHeight));
					m_verticalPosition = Mathf.Clamp(m_verticalPosition, 0, 7);
				}

				targetY = ((float)m_faderGroup.m_faderHeight / 7.0f * (float)m_verticalPosition) /* + ((float)m_faderGroup.m_faderHeight / 16.0f) */ + m_faderGroup.m_faderOffset;


				// calculate new z coordinate

				// snap horizontal position on manual change in scene view
				if (transform.position.z != m_prevZ){
					m_playerLane = (int)(((Constants.NUMBER_OF_PLAYERS/2) - (transform.position.z + (float)m_faderGroup.m_faderPadding/2.0f)) / m_faderGroup.m_faderPadding);
					m_playerLane = Mathf.Clamp(m_playerLane, 0, 7);
				}

				targetZ = (((Constants.NUMBER_OF_PLAYERS/2) - m_playerLane) * m_faderGroup.m_faderPadding - (float)m_faderGroup.m_faderPadding/2.0f);


				// set position
				transform.position = new Vector3(targetX, targetY, targetZ);

				// update helpers
				m_prevX = transform.position.x;
				m_prevY = transform.position.y;
				m_prevZ = transform.position.z;
			}

		}
	}

	void OnDrawGizmosSelected(){
		if (m_inEditMode || m_snapInPlayMode){
			Gizmos.color = m_gizmoColor;

			// vertical lines
			for (int y = 0; y < Constants.NUMBER_OF_PLAYERS; y++){
				
				Vector3 min = new Vector3(	transform.position.x,
											(m_faderGroup.m_faderOffset - 1),
											((y - Constants.NUMBER_OF_PLAYERS/2) * m_faderGroup.m_faderPadding + (float)m_faderGroup.m_faderPadding/2));

				Vector3 max = min + Vector3.up * (m_faderGroup.m_faderHeight + 2);

				Gizmos.DrawLine(min, max);
			}

			// horizontal lines
			for (int z = 0; z < Constants.NUMBER_OF_PLAYERS; z++){

				Vector3 min = new Vector3(	transform.position.x,
					((float)z * (float)m_faderGroup.m_faderHeight / (float)(Constants.NUMBER_OF_PLAYERS - 1)) /*+ ((float)m_faderGroup.m_faderHeight / (float)Constants.NUMBER_OF_PLAYERS)/2.0f*/ + m_faderGroup.m_faderOffset,
					m_faderGroup.m_faderPadding * Constants.NUMBER_OF_PLAYERS / 2);

				Vector3 max = min + Vector3.back * m_faderGroup.m_faderPadding * Constants.NUMBER_OF_PLAYERS;

				Gizmos.DrawLine(min, max);
			}

			#if UNITY_EDITOR
			// label
			Handles.Label(transform.position + Vector3.one, m_playerLane + ", " + m_verticalPosition);
			#endif
		}
	}
}