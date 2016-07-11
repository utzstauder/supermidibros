using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/**
 * Can affect any OnRhythmPeriodic classes. Put this on the same hierarchy level as any affectable class.
 */
public class OnRhythmPeriodicAffectorAudioChannel : OnRhythmPeriodicAffector {

	[Range(0, 2)]
	public int category = 0;
	[Range(0, 3)]
	public int instrument = 0;
	[Range(0, 3)]
	public int variation = 0;

	private AudioManager	m_audioManager;

	protected override void Awake () {
		base.Awake();

		if (!m_audioManager){
			m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
			if (!m_audioManager){
				Debug.LogError("No AudioManager found in this scene!");
			}
		}
	}

	protected override void Update ()
	{
		base.Update ();

		active = m_audioManager.IsChannelActive(category, instrument, variation);
	}
}
