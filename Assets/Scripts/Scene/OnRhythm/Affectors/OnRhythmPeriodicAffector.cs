using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/**
 * Can affect any OnRhythmPeriodic classes. Put this on the same hierarchy level as any affectable class.
 */
public class OnRhythmPeriodicAffector : MonoBehaviour {

	public bool m_isAffecting							= true;
	public Enums.AudioMixerExposedParams m_affectedBy;
	public Vector2 m_volumeRange						= new Vector2 (-80.0f, 0.0f);

	private AudioManager	m_audioManager;
	private AudioMixer		m_audioMixer;

	private float			m_volume;

	void Awake () {
		if (!m_audioManager){
			m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
			if (!m_audioManager){
				Debug.LogError("No AudioManager found in this scene!");
			} else {
				m_audioMixer = m_audioManager.GetAudioMixer();
			}
		}
	}

	/**
	 * Returns a value between 0.0f and 1.0f depending on the volume of the affecting AudioMixerGroup.
	 */
	public float GetMultiplier(){
		if (m_audioMixer.GetFloat(m_affectedBy.ToString(), out m_volume)){
			return Mathf.InverseLerp(m_volumeRange.x, m_volumeRange.y, m_volume);
		}
		return 1.0f;
	}

	/**
	 * Return true if the script is curently affecting anything.
	 */
	public bool IsAffecting(){
		return m_isAffecting;
	}
}
