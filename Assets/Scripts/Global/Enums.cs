using UnityEngine;
using System.Collections;

/**
 * Contains every global enum type
 */
public static class Enums {

	/**
	 * 
	 */
	public enum SyncType {Bar, Beat, SubBeat};

	/**
	 *	Each value represents one AudioMixerGroup
	 */
	public enum AudioMixerExposedParams {
		volume_rhythm_0,
		volume_rhythm_1,
		volume_rhythm_2,
		volume_rhythm_3,
		volume_rhythm_4,
		volume_rhythm_5,
		volume_rhythm_6,
		volume_rhythm_7,
		volume_harmony_0,
		volume_harmony_1,
		volume_harmony_2,
		volume_harmony_3,
		volume_harmony_4,
		volume_harmony_5,
		volume_harmony_6,
		volume_harmony_7,
		volume_melody_0,
		volume_melody_1,
		volume_melody_2,
		volume_melody_3,
		volume_melody_4,
		volume_melody_5,
		volume_melody_6,
		volume_melody_7,
	};
}
