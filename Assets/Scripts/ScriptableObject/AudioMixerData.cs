using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[CreateAssetMenu(menuName = "Super MIDI Bros./AudioMixerData", fileName = "AudioMixerData")]
public class AudioMixerData : ScriptableObject {

	public AudioMixer mainMixer;
	public AudioMixerSnapshot standardSnapshot;
	public AudioMixerGroup metronomeTrack;
	public AudioMixerCategory[] categories = new AudioMixerCategory[Constants.AUDIO_CATEGORIES] {
		new AudioMixerCategory("Rhythm"),
		new AudioMixerCategory("Harmony"),
		new AudioMixerCategory("Melody"),
	};

	public AudioMixerGroup GetAudioMixerGroup(int category, int instrument, int variation){
		return categories[category].GetInstrument(instrument).GetAudioMixerGroup(variation);
	}

	[System.Serializable]
	public struct AudioMixerCategory{
		public string name;
		public AudioMixerInstrument[] instruments;

		public AudioMixerCategory(string _name){
			name = _name;
			instruments = new AudioMixerInstrument[Constants.INSTRUMENT_GROUPS];
			for (int i = 0; i < instruments.Length; i++){
				instruments[i].name = "Instrument_" + i;
			}
		}

		public AudioMixerInstrument GetInstrument(int instrument){
			return instruments[instrument];
		}
	}

	[System.Serializable]
	public struct AudioMixerInstrument{
		public string name;
		public AudioMixerGroup[] variations;

		public AudioMixerInstrument(string _name){
			name = _name;
			variations = new AudioMixerGroup[Constants.VARIATIONS];
			for (int v = 0; v < variations.Length; v++){
				variations[v].name = "Variation_" + v;
			}
		}

		public AudioMixerGroup GetAudioMixerGroup(int variation){
			return variations[variation];
		}
	}
}
