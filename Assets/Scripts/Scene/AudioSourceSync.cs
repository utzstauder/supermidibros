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
		
	void Update () {
	
	}


	#region public functions

	public void Play(){
		m_audioSource.Play();
	}

	public void Pause(){
		m_audioSource.Pause();
	}

	public void Stop(){
		m_audioSource.Stop();
	}

	public void SyncToMaster(AudioManager _reference){
		if (m_audioSource.clip != null){
			if (m_isLoop){
				
				if (m_manualLoopArea){
					// manual loop area
					SetTimeSamples((_reference.GetCurrentTimeSamples() % (m_loopLength * _reference.GetSamplesPerBar())) +
									m_loopOffset * _reference.GetSamplesPerBar());
				} else {
					// automatically loop through entire track
					SetTimeSamples(Mathf.Clamp((_reference.GetCurrentTimeSamples() % m_audioSource.clip.samples), 0, m_audioSource.clip.samples));
				}

			} else {
				
				// simple sync
				SetTimeSamples(_reference.GetCurrentTimeSamples());

			}
		}
	}

	public void SyncToMaster(int _timeSamples, int _bpm ){
		
	}

	public void SetTimeSamples(int _timeSamples){
		m_audioSource.timeSamples = _timeSamples;
	}

	#endregion


}
