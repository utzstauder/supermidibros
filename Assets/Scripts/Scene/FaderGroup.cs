using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FaderGroup : MonoBehaviour {

	[Range(1,32)]
	public int m_faderHeight = 16;
	[Range(0,4)]
	public int m_faderOffset = 0;
	[Range(1,4)]
	public int m_faderPadding = 2;

	[Header("Gizmos")]
	public Color m_faderColor = Color.blue;

	private Transform[] m_faderGroup = new Transform[Constants.NUMBER_OF_PLAYERS];

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
		}
	}

	#region public getter

	public Vector3 GetLocalPositionOfFader(int _faderId){
		return m_faderGroup[_faderId].localPosition;
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
		}
	}
}
