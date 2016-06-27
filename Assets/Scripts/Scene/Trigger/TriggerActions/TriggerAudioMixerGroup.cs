using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class TriggerAudioMixerGroup : TriggerTarget {

	public Enums.AudioMixerExposedParams m_audioMixerGroup;

	private AudioManager m_audioManager;
	private AudioMixer m_audioMixer;

	[Range(-80.0f, 20.0f)]
	public float m_targetVolumeSuccess = 0.0f;
	[Range(-80.0f, 20.0f)]
	public float m_targetVolumeFailure = -80.0f;

	protected override void Awake () {
		base.Awake();

		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (m_audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {
			m_audioMixer = m_audioManager.GetAudioMixer();
		}
	}
		
	protected override void ActionSuccess (Trigger _reference){
		base.ActionSuccess (_reference);

		m_audioMixer.SetFloat(m_audioMixerGroup.ToString(), m_targetVolumeSuccess);

		//gameObject.SetActive(false);
	}

	protected override void ActionFailure (Trigger _reference){
		base.ActionFailure (_reference);

		m_audioMixer.SetFloat(m_audioMixerGroup.ToString(), m_targetVolumeFailure);

		//gameObject.SetActive(false);
	}

	void OnDrawGizmos(){
		#if UNITY_EDITOR
		Handles.Label(transform.position - Vector3.one, m_audioMixerGroup.ToString());
		#endif
	}
}
