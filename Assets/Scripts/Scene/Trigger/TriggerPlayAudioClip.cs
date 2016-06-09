using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TriggerTarget))]
[RequireComponent(typeof(AudioSource))]
public class TriggerPlayAudioClip : MonoBehaviour {

	public AudioClip m_audioClip;
	public bool m_loop;

	private TriggerTarget m_triggerTarget;
	private AudioSource m_audioSource;

	// Use this for initialization
	void Awake () {
		m_triggerTarget = GetComponent<TriggerTarget>();
		m_audioSource = GetComponent<AudioSource>();

		m_triggerTarget.OnReceiveTrigger += OnReceiveTrigger;

		m_audioSource.playOnAwake = false;
		m_audioSource.clip = m_audioClip;
	}

	void OnReceiveTrigger(){
		if (m_loop){
			m_audioSource.loop = true;
		} else {
			m_audioSource.loop = false;
		}

		m_audioSource.Play();
	}
}
