using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Super MIDI Bros./PatternData", fileName = "PatternData")]
public class PatternData : ScriptableObject {

	public GameObject rhythmPrefab;
	public GameObject harmonyPrefab;
	public GameObject melodyPrefab;

	public Color[] categoryColors	= new Color[Constants.AUDIO_CATEGORIES];
	public Color[] instrumentColors	= new Color[Constants.INSTRUMENT_GROUPS];
	public Color[] variationColors	= new Color[Constants.VARIATIONS];
}
