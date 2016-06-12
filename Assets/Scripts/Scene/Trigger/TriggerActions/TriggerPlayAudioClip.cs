using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TriggerPlayAudioClip : TriggerDoAction {

	public AudioClip m_audioClip;
	public bool m_loop;

	private AudioSource m_audioSource;

	protected override void Awake () {
		base.Awake();

		m_audioSource = GetComponent<AudioSource>();

		m_audioSource.playOnAwake = false;
		m_audioSource.clip = m_audioClip;
	}

	protected override void Action(){
		base.Action();

		if (m_loop){
			m_audioSource.loop = true;
		} else {
			m_audioSource.loop = false;
		}

		m_audioSource.Play();
	}
}
