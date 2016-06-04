using UnityEngine;
using System.Collections;

public class PlayerGroup : MonoBehaviour {

	[Range(0.01f, 100.0f)]
	public float m_lerpSpeed = 10.0f;

	private float[] m_playerZOffset = {0, 0, 0, 0, 0, 0, 0, 0};
	private Vector3 m_targetPosition;

	[SerializeField]
	private FaderGroup m_faderGroup;
	private Transform[] m_playerGroup;

	void Awake () {
		if (!m_faderGroup){
			m_faderGroup = GameObject.Find("FaderGroup").GetComponent<FaderGroup>();
			if (!m_faderGroup){
				Debug.LogError("FaderGroup not found!");
				Destroy(this);
			}
		}

		m_playerGroup = new Transform[transform.childCount];
		for (int i = 0; i < transform.childCount; i++){
			m_playerGroup[i] = transform.GetChild(i);
			m_playerGroup[i].gameObject.name = "Player_" + i;
		}
	}

	void Start(){
		for (int i = 0; i < m_playerGroup.Length; i++){
			SetPositionOfPlayer(i, m_faderGroup.GetLocalPositionOfFader(i));
		}
	}

	void Update(){
		UpdateTargetPositionOfAllPlayers();
	}

	#region private functions

	void UpdateTargetPositionOfAllPlayers(){
		for (int i = 0; i < m_playerGroup.Length; i++){
			UpdateTargetPositionOfPlayer(i);
		}
	}

	void UpdateTargetPositionOfPlayer(int _playerId){
		m_targetPosition = m_faderGroup.GetLocalPositionOfFader(_playerId);
		m_playerGroup[_playerId].localPosition = Vector3.Lerp(m_playerGroup[_playerId].localPosition, m_targetPosition, Time.deltaTime * m_lerpSpeed);
	}

	void SetPositionOfPlayer(int _playerId, Vector3 _position){
		m_playerGroup[_playerId].localPosition = _position;
	}

	#endregion

}
