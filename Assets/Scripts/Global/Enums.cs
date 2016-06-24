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
		Kick_Variation_1,
		Kick_Variation_2,
		Kick_Variation_3,
		Kick_Variation_4,
		Snare_Variation_1,
		Snare_Variation_2,
		Snare_Variation_3,
		Snare_Variation_4,
		HiHat_Variation_1,
		HiHat_Variation_2,
		HiHat_Variation_3,
		HiHat_Variation_4,
		Percussion_Variation_1,
		Percussion_Variation_2,
		Percussion_Variation_3,
		Percussion_Variation_4,
	};
}
