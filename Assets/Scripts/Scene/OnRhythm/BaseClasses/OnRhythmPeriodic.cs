using UnityEngine;
using System.Collections;

public class OnRhythmPeriodic : MonoBehaviour {

	public Enums.SyncType m_syncType	= Enums.SyncType.Bar;
	public AnimationCurve m_curve		= AnimationCurve.Linear(0, 0, 1, 1);

	private AudioManager				m_audioManager;
	private OnRhythmPeriodicAffector	m_affector;
	public bool							m_isAffected	= true;
	private float 						m_tmp;

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
	
	// Update is called once per frame
	void Update () {
		switch (m_syncType){
		case Enums.SyncType.Bar:		m_tmp = m_audioManager.GetCurrentBarTime();		break;
		case Enums.SyncType.Beat:		m_tmp = m_audioManager.GetCurrentBeatTime();	break;
		case Enums.SyncType.SubBeat:	m_tmp = m_audioManager.GetCurrentSubBeatTime(); break;
		}

		m_tmp = m_tmp - (int)m_tmp;

		if (m_isAffected && m_affector != null){
			Action(m_curve.Evaluate(m_tmp) * m_affector.GetMultiplier());
		} else {
			Action(m_curve.Evaluate(m_tmp));
		}
	}

	// this is called every frame
	protected virtual void Action(float _timer){
		
	}
}
