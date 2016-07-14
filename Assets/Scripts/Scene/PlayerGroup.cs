using UnityEngine;
using System.Collections;

public class PlayerGroup : MonoBehaviour {

	[Range(0.01f, 100.0f)]
	public float m_lerpSpeed = 10.0f;

	public Vector3 m_lookAtOffset = Vector3.zero;

	public GameObject	m_playerPrefab;
//	public GameObject[] m_playerCharacterPrefabs;
	public int startIndex = 0;
	public PlayerCharacterData playerCharacterData;

	private Vector3 m_targetPosition;
	private FaderGroup m_faderGroup;
	private GameObject[] m_playerGroup;


	void Awake () {
		if (!m_faderGroup){
			m_faderGroup = GameObject.Find("FaderGroup").GetComponent<FaderGroup>();
			if (!m_faderGroup){
				Debug.LogError("FaderGroup not found!");
				Destroy(this);
			}
		}

		startIndex = Mathf.Clamp(startIndex, 0, playerCharacterData.playerCharacters.Length - 1);

		m_playerGroup = new GameObject[Constants.NUMBER_OF_PLAYERS];
		for (int i = 0; i < m_playerGroup.Length; i++){
			m_playerGroup[i] = InstantiatePlayer(i);
		}

	}

	void Start(){
		for (int i = 0; i < m_playerGroup.Length; i++){
			SetPositionOfPlayer(i, m_faderGroup.GetLocalPositionOfFader(i));
		}
	}

	void Update(){
		UpdateTargetPositionOfAllPlayers();
		if (m_lookAtOffset.magnitude > 0){
			UpdateLookAtOfAllPlayers();
		}
	}

	#region private functions

	GameObject InstantiatePlayer(int _playerId){
		GameObject _player = Instantiate(m_playerPrefab, m_faderGroup.GetLocalPositionOfFader(_playerId), Quaternion.identity) as GameObject;
		_player.name = "Player_" + _playerId;
		_player.transform.parent = this.transform;

//		GameObject _playerCharacter = Instantiate(m_playerCharacterPrefabs[_playerId], Vector3.zero, Quaternion.identity) as GameObject;
//		_playerCharacter.transform.parent = _player.transform;
//		_playerCharacter.transform.localPosition = Vector3.zero;
//
//		OnRhythmPeriodicAffectorAlignment[] alignmentAffectors = _playerCharacter.GetComponentsInChildren<OnRhythmPeriodicAffectorAlignment>();
//		for (int i = 0; i < alignmentAffectors.Length; i++){
//			alignmentAffectors[i].playerId = _playerId;
//		}
//
//		if (_playerId >= (Constants.NUMBER_OF_PLAYERS / 2)){
//			_playerCharacter.GetComponent<RotateOnRhythmPeriodic>().m_rotation *= -1;
//		}

		for (int c = 0; c < playerCharacterData.playerCharacters.Length; c++){
			GameObject _playerCharacter = Instantiate(playerCharacterData.playerCharacters[c].prefab, Vector3.zero, Quaternion.identity) as GameObject;
			_playerCharacter.transform.parent = _player.transform;
			_playerCharacter.transform.localPosition = Vector3.zero;

			OnRhythmPeriodicAffectorAlignment[] alignmentAffectors = _playerCharacter.GetComponentsInChildren<OnRhythmPeriodicAffectorAlignment>();
			for (int i = 0; i < alignmentAffectors.Length; i++){
				alignmentAffectors[i].playerId = _playerId;
			}

			if (_playerId >= (Constants.NUMBER_OF_PLAYERS / 2)){
				_playerCharacter.GetComponent<RotateOnRhythmPeriodic>().m_rotation *= -1;
			}

			if (c != startIndex){
				_playerCharacter.gameObject.SetActive(false);
			}
		}

		return _player;
	}

	void UpdateTargetPositionOfAllPlayers(){
		for (int i = 0; i < m_playerGroup.Length; i++){
			UpdateTargetPositionOfPlayer(i);
		}
	}

	void UpdateLookAtOfAllPlayers(){
		for (int i = 0; i < m_playerGroup.Length; i++){
			UpdateLookAtOfPlayer(i);
		}
	}

	void UpdateTargetPositionOfPlayer(int _playerId){
		m_targetPosition = m_faderGroup.GetLocalPositionOfFader(_playerId);
		m_playerGroup[_playerId].transform.localPosition = Vector3.Lerp(m_playerGroup[_playerId].transform.localPosition, m_targetPosition, Time.deltaTime * m_lerpSpeed);
	}

	void SetPositionOfPlayer(int _playerId, Vector3 _position){
		m_playerGroup[_playerId].transform.localPosition = _position;
	}

	void UpdateLookAtOfPlayer(int _playerId){
		m_playerGroup[_playerId].transform.LookAt(	Vector3.forward * m_playerGroup[_playerId].transform.position.z +
													Vector3.right * m_playerGroup[_playerId].transform.position.x + m_lookAtOffset +
			(Vector3.up * m_faderGroup.GetRelativePositionOfFader(_playerId) * Constants.FADER_HEIGHT) +
			Vector3.up * Constants.FADER_OFFSET);
	}

	#endregion

	void OnDrawGizmos(){
//		for (int i = 0; i < m_playerGroup.Length; i++){
//			Gizmos.DrawLine(m_playerGroup[i].position, m_playerGroup[i].position + m_lookAtOffset);
//		}
	}

}
