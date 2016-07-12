using UnityEngine;
using System.Collections;

public class OnRhythm : MonoBehaviour {

	public Enums.SyncType m_syncType	= Enums.SyncType.Bar;
	public int m_syncFrequency			= 1;

	private AudioManager m_audioManager;
	private OnRhythmPeriodicAffector	m_affector;
	public bool							m_isAffected	= true;

	// Use this for initialization
	protected virtual void Awake () {
		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (m_audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {

		}

		if ((m_affector = GetComponent<OnRhythmPeriodicAffector>()) != true){
			// no affector found
		}
	}

	protected virtual void OnEnable(){
		if (m_audioManager != null){
			switch(m_syncType){
			case Enums.SyncType.Bar:		m_audioManager.OnBar 		+= OnReceiveRhythm; break;
			case Enums.SyncType.Beat:		m_audioManager.OnBeat 		+= OnReceiveRhythm; break;
			case Enums.SyncType.SubBeat:	m_audioManager.OnSubBeat 	+= OnReceiveRhythm; break;
			default: break;
			}
		}
	}

	protected virtual void OnDisable(){
		if (m_audioManager != null){
			switch(m_syncType){
			case Enums.SyncType.Bar:		m_audioManager.OnBar 		-= OnReceiveRhythm; break;
			case Enums.SyncType.Beat:		m_audioManager.OnBeat 		-= OnReceiveRhythm; break;
			case Enums.SyncType.SubBeat:	m_audioManager.OnSubBeat 	-= OnReceiveRhythm; break;
			default: break;
			}
		}
	}
		
	protected virtual void OnReceiveRhythm(int _value){
		if (_value % m_syncFrequency == 0){
			if (m_isAffected && m_affector != null){
				if (m_affector.GetMultiplier() > 0){
					Action();
				}
			} else {
				Action();
			}
		}


	}

	public virtual void Action(){
	
	}
}
