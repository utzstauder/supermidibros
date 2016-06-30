#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SnapToGrid : MonoBehaviour {

	[Header("X")]
	public bool m_snapX		= true;
	public bool m_lockX		= false;
	public int 	m_bar 		= 1;
	public int 	m_beat		= 0;
	public int 	m_subBeat 	= 0;
	private float m_prevX;

	[Header("Y")]
	public bool m_snapY		= true;
	public bool m_lockY		= false;
	[Range(0,Constants.VERTICAL_POSITIONS-1)]
	public int m_verticalPosition = 0;
	private float m_prevY;

	[Header("Z")]
	public bool m_snapZ		= true;
	public bool m_lockZ		= false;
	[Range(0,Constants.NUMBER_OF_PLAYERS-1)]
	public int m_playerLane = 0;
	private float m_prevZ;

	private bool m_lockPosition		= false;
	private bool m_snapInPlayMode	= false;
	private bool m_moveInEditMode	= false;

	private AudioManager 	m_audioManager;
	//private FaderGroup		m_faderGroup;
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
//		if (!m_faderGroup){
//			m_faderGroup = GameObject.Find("FaderGroup").GetComponent<FaderGroup>();
//			if (!m_faderGroup){
//				Debug.LogError("No FaderGroup found in this scene!");
//			}
//		}

		if (Application.isPlaying){
			m_inEditMode = false;
			m_lockPosition = true;
		} else {
			m_inEditMode = true;
		}

		// stay on awake x coordinate
		CalculateFromWorldX(transform.position.x);


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
//				if (!m_faderGroup){
//					m_faderGroup = GameObject.Find("FaderGroup").GetComponent<FaderGroup>();
//					if (!m_faderGroup){
//						Debug.LogError("No FaderGroup found in this scene!");
//					}
//				}

				UpdatePosition();
			}

		}
	}

	public void UpdatePosition(){
		// calculate new x coordinate

		// snap x position on manual change in scene view
		if (transform.position.x != m_prevX && m_moveInEditMode){
			CalculateFromWorldX(transform.position.x);
		}

		// clamp input boundaries
		if (m_audioManager != null){
			m_bar = Mathf.Clamp(m_bar, 1, m_bar);
			m_beat = Mathf.Clamp(m_beat, 1, m_audioManager.GetTimeSignatureUpper());
			m_subBeat = Mathf.Clamp(m_subBeat, 1, m_audioManager.GetUnitsPerBeat());
		}

		if (m_snapX && m_audioManager != null){
			targetX = ((m_bar - 1) * m_audioManager.GetUnitsPerBeat() * m_audioManager.GetTimeSignatureUpper())
				+ ((m_beat - 1) * m_audioManager.GetUnitsPerBeat())
				+ (m_subBeat - 1);
		} else if (m_lockX){
			targetX = 0;
		} else {
			targetX = transform.localPosition.x;
		}


		// calculate new y coordinate

		// snap vertical position on manual change in scene view
		if (transform.position.y != m_prevY && m_moveInEditMode){
			CalculateFromWorldY(transform.position.y);
		}

		if (m_snapY){
			targetY = ((float)Constants.FADER_HEIGHT / (float)(Constants.VERTICAL_POSITIONS-1) * (float)m_verticalPosition) /* + ((float)m_faderGroup.m_faderHeight / 16.0f) */ + Constants.FADER_OFFSET;
		} else if (m_lockY){
			targetY = 0;
		} else {
			targetY = transform.localPosition.y;
		}


		// calculate new z coordinate

		// snap horizontal position on manual change in scene view
		if (transform.position.z != m_prevZ && m_moveInEditMode){
			CalculateFromWorldZ(transform.position.z);
		}

		if (m_snapZ){
			targetZ = (((Constants.NUMBER_OF_PLAYERS/2) - m_playerLane) * Constants.FADER_PADDING - (float)Constants.FADER_PADDING/2.0f);
		} else if (m_lockZ){
			targetZ = 0;
		} else {
			targetZ = transform.localPosition.z;
		}


		// set position
		transform.localPosition = new Vector3(targetX, targetY, targetZ);

		// update helpers
		m_prevX = transform.position.x;
		m_prevY = transform.position.y;
		m_prevZ = transform.position.z;
	}

	public void SetPosition(int bar, int beat, int subBeat){
		m_bar = bar;
		m_beat = beat;
		m_subBeat = beat;

		UpdatePosition();
	}

	public void SetPosition(int horizontal, int vertical){
		m_playerLane = horizontal;
		m_verticalPosition = vertical;

		UpdatePosition();
	}

	#region calculate from world

	public void CalculateFromWorldX(float _x){
		targetX = Mathf.Round(_x);

		m_bar		= (int)_x / (m_audioManager.GetUnitsPerBeat() * m_audioManager.GetTimeSignatureUpper()) + 1;
		m_beat		= ((int)_x / m_audioManager.GetUnitsPerBeat()) % m_audioManager.GetTimeSignatureUpper() + 1;
		m_subBeat	= (int)_x % m_audioManager.GetUnitsPerBeat() + 1;
	}

	public void CalculateFromWorldY(float _y){
		m_verticalPosition = Mathf.RoundToInt(((Constants.VERTICAL_POSITIONS-1) * (_y /*- ((float)m_faderGroup.m_faderHeight / 16.0f)*/ + (float)Constants.FADER_OFFSET) /
			(float)Constants.FADER_HEIGHT));
		m_verticalPosition = Mathf.Clamp(m_verticalPosition, 0, (Constants.VERTICAL_POSITIONS-1));
	}

	public void CalculateFromWorldZ(float _z){
		m_playerLane = (int)(((Constants.NUMBER_OF_PLAYERS/2) - (_z + (float)Constants.FADER_PADDING/2.0f)) / Constants.FADER_PADDING);
		m_playerLane = Mathf.Clamp(m_playerLane, 0, (Constants.NUMBER_OF_PLAYERS-1));
	}

	#endregion


	#region static functions

	/**
	 * Returns a position in the pattern grid as world coordinates
	 * 
	 */
	public static Vector3 GridToWorldCoord(float xWorld, int y, int z){
		float newY = (float)Constants.FADER_HEIGHT / (float)(Constants.VERTICAL_POSITIONS - 1) * (float)y + Constants.FADER_OFFSET;
		float newZ = ((Constants.NUMBER_OF_PLAYERS/2) - z) * Constants.FADER_PADDING - (float)Constants.FADER_PADDING/2.0f;
		return new Vector3(xWorld, newY, newZ);
	}
	public static Vector3[,] GridAsWorldCoords(){
		Vector3[,] grid = new Vector3[Constants.NUMBER_OF_PLAYERS, Constants.VERTICAL_POSITIONS];
		for (int x = 0; x < grid.GetLength(0); x++){
			for (int y = 0; y < grid.GetLength(1); y++){
				grid[x,y] = GridToWorldCoord(0, y, x);
			}
		}
		return grid;
	}

	#endregion


	void OnDrawGizmosSelected(){
		if (m_inEditMode || m_snapInPlayMode){
			Gizmos.color = m_gizmoColor;

			// vertical lines
			for (int y = 0; y < Constants.NUMBER_OF_PLAYERS; y++){
				
				Vector3 min = new Vector3(	transform.position.x,
					(Constants.FADER_OFFSET - 1),
					((y - Constants.NUMBER_OF_PLAYERS/2) * Constants.FADER_PADDING + (float)Constants.FADER_PADDING/2));

				Vector3 max = min + Vector3.up * (Constants.FADER_HEIGHT + 2);

				Gizmos.DrawLine(min, max);
			}

			// horizontal lines
			for (int z = 0; z < Constants.VERTICAL_POSITIONS; z++){

				Vector3 min = new Vector3(	transform.position.x,
					((float)z * (float)Constants.FADER_HEIGHT / (float)(Constants.NUMBER_OF_PLAYERS - 1)) /*+ ((float)m_faderGroup.m_faderHeight / (float)Constants.NUMBER_OF_PLAYERS)/2.0f*/ + Constants.FADER_OFFSET,
					Constants.FADER_PADDING * Constants.NUMBER_OF_PLAYERS / 2);

				Vector3 max = min + Vector3.back * Constants.FADER_PADDING * Constants.NUMBER_OF_PLAYERS;

				Gizmos.DrawLine(min, max);
			}

			#if UNITY_EDITOR
			// label
			Handles.Label(transform.position + Vector3.one, m_playerLane + ", " + m_verticalPosition);
			#endif
		}
	}
}