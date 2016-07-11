using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/**
 * Can affect any OnRhythmPeriodic classes. Put this on the same hierarchy level as any affectable class.
 */
public class OnRhythmPeriodicAffectorAlignment : OnRhythmPeriodicAffector {

	public int playerId = 0;
	private FaderGroup faderGroup;



	protected override void Awake () {
		base.Awake();

		faderGroup = GameObject.Find("FaderGroup").GetComponent<FaderGroup>();

		if (faderGroup == null){
			Debug.LogError("No faderGroup found!");
			this.enabled = false;
		}
	}

	protected override void Update(){
		active = faderGroup.GetAlignmentInfo()[playerId];

		base.Update();
	}

	/**
	 * Returns a value between 0.0f and 1.0f depending on the volume of the affecting AudioMixerGroup.
	 */
	public override float GetMultiplier(){
		return (active) ? affectorAmount : 0;
	}
}
