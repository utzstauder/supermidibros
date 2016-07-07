using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/**
 * Can affect any OnRhythmPeriodic classes. Put this on the same hierarchy level as any affectable class.
 */
public class OnRhythmPeriodicAffector : MonoBehaviour {

	[Range(0, 2)]
	public int category = 0;
	[Range(0, 3)]
	public int instrument = 0;
	[Range(0, 3)]
	public int variation = 0;

	private AudioManager	m_audioManager;

	void Awake () {
		if (!m_audioManager){
			m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
			if (!m_audioManager){
				Debug.LogError("No AudioManager found in this scene!");
			}
		}
	}

	/**
	 * Returns a value between 0.0f and 1.0f depending on the volume of the affecting AudioMixerGroup.
	 */
	public float GetMultiplier(){
		return (m_audioManager.IsChannelActive(category, instrument, variation)) ? 1.0f : 0;
	}
}
