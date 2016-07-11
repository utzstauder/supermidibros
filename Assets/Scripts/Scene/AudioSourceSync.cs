using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceSync : MonoBehaviour {

	private AudioSource m_audioSource;

	public bool 		m_isLoop			= false;
	public bool			m_manualLoopArea	= false;

	// loops must be a multiple of one bar length
	public int 			m_loopOffset		= 0;
	public int 			m_loopLength		= 1;

	void Awake(){
		m_audioSource = GetComponent<AudioSource>();
		m_audioSource.playOnAwake = false;

		if (m_isLoop){
			m_audioSource.loop = true;
		}
	}

	#region public functions

	public void Play(){
		if (m_audioSource != null){
			m_audioSource.Play();
		}
	}

	public void Pause(){
		if (m_audioSource != null){
			m_audioSource.Pause();
		}
	}

	public void Stop(){
		if (m_audioSource != null){
			m_audioSource.Stop();
		}
	}

	public void SetLoop(bool loop){
		m_isLoop = loop;
		m_audioSource.loop = m_isLoop;
	}

	public void SyncToMaster(AudioManager _reference){
		if (m_audioSource.clip != null){
			if (m_isLoop){
				
				if (m_manualLoopArea){
					// manual loop area
					SetTimeSamples((int)(_reference.GetCurrentAudioTimeSamples() % (m_loopLength * _reference.GetSamplesPerBar())) +
						m_loopOffset * _reference.GetSamplesPerBar());
				} else {
					// automatically loop through entire track
					SetTimeSamples((_reference.GetCurrentAudioTimeSamples() % m_audioSource.clip.samples));
				}

			} else {
				
				// simple sync
				SetTimeSamples(_reference.GetCurrentAudioTimeSamples());

			}
		}
	}

	public void SetTimeSamples(int _timeSamples){
		//_timeSamples = Mathf.Clamp(_timeSamples, 0, m_audioSource.clip.samples);
		m_audioSource.timeSamples = _timeSamples;
	}

	public void SetTime(float time){
		time = Mathf.Clamp(time, 0, m_audioSource.clip.length);
		m_audioSource.time = time;
	}

	#endregion


}
