using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FaderGroup : MonoBehaviour {

	[Header("Fader Array")]
	[Range(1,32)]
	public int m_faderHeight = 16;
	[Range(0,4)]
	public int m_faderOffset = 0;
	[Range(1,4)]
	public int m_faderPadding = 2;


	[Header("Collision Detection")]
	[Range(0.01f, 0.5f)]
	public float m_detectionRange = .5f;
	private Collider[] m_collider;


	[Header("Gizmos")]
	public Color m_faderColor = Color.blue;


	private Transform[] m_faderGroup = new Transform[Constants.NUMBER_OF_PLAYERS];
	private AudioManager m_audioManager;
	private bool m_inEditMode = true;

	// Use this for initialization
	void Awake () {
		if (Application.isPlaying){
			m_inEditMode = false;
		} else {
			m_inEditMode = true;
		}

		m_faderGroup = new Transform[transform.childCount];
		for (int i = 0; i < transform.childCount; i++){
			m_faderGroup[i] = transform.GetChild(i);
			m_faderGroup[i].gameObject.name = "Fader_" + i;
		}

		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (m_audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {
			m_audioManager.OnSubBeat += CheckForCollision;
		}


	}
	
	// Update is called once per frame
	void Update () {
		if (!m_inEditMode){
			// TODO: this can be optimized, listen for *changes* in player input
			// input
			for (int i = 0; i < m_faderGroup.Length; i++){
				m_faderGroup[i].localPosition = Vector3.up * m_faderHeight * MIDIInputManager.instance.GetInputOfPlayer(i)
					+ Vector3.up * m_faderOffset
					+ Vector3.back * ((i - Constants.NUMBER_OF_PLAYERS/2) * m_faderPadding + (float)m_faderPadding/2);
			}
		} else {
			for (int i = 0; i < m_faderGroup.Length; i++){
				m_faderGroup[i].localPosition = Vector3.zero
					+ Vector3.up * m_faderOffset
					+ Vector3.back * ((i - Constants.NUMBER_OF_PLAYERS/2) * m_faderPadding + (float)m_faderPadding/2);
			}
		}
	}

	#region private functions

	/**
	 * This function will check for collision and is called on every subbeat
	 */
	void CheckForCollision(int _subBeat){
		for (int i = 0; i < m_faderGroup.Length; i++){
			m_collider = Physics.OverlapSphere(m_faderGroup[i].position, m_detectionRange);
			for (int c = 0; c < m_collider.Length; c++){
				if (m_collider[c].GetComponent<Trigger>()){
					m_collider[c].GetComponent<Trigger>().OnCollision(i);
				}
			}
		}
	}

	#endregion

	#region public getter

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
