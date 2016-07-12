using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/**
 * Can affect any OnRhythmPeriodic classes. Put this on the same hierarchy level as any affectable class.
 */
public class OnRhythmPeriodicAffectorCombo : OnRhythmPeriodicAffector {

	public int minCombo = 0;
	public int maxCombo = 0;

	private float t = 0;

	void OnValidate(){
		if (minCombo < 0){
			minCombo = 0;
		}

		if (maxCombo <= minCombo){
			maxCombo = minCombo + 1;
		}
	}

	protected override void Awake () {
		base.Awake();
	}

	protected override void Update(){
		active = (GameManager.instance.GetCombo() >= minCombo);

		if (active){
			t = Mathf.InverseLerp(minCombo, maxCombo, GameManager.instance.GetCombo());
			affectorAmount = Mathf.Lerp(0, 1.0f, t);
		}
	}

	/**
	 * Returns a value between 0.0f and 1.0f depending on the volume of the affecting AudioMixerGroup.
	 */
	public override float GetMultiplier(){
		return (active) ? affectorAmount : 0;
	}
}
