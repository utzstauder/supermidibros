using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using CustomDataTypes;

[CreateAssetMenu (menuName = "Super MIDI Bros./SoundSet", fileName = "SoundSet", order = 100)]
public class SoundSet : ScriptableObject {

	public int bpm;
//	public int timeSignatureUpper = 4;
//	public int timeSignatureLower = 4;

	public AudioMixerSnapshot audioMixerSnapshot;

	public AudioCategory[] m_audioCategories;

	public SoundSet(){
		m_audioCategories = new AudioCategory[Constants.AUDIO_CATEGORIES]{
			new AudioCategory("Rhythm",
				"Kick", "Snare", "Hi-Hat", "Percussion"),
			new AudioCategory("Harmony"),
			new AudioCategory("Melody"),
		};
	}


	#region public functions

	public int GetRandomIndex(){
		return Random.Range(0, m_audioCategories.Length);
	}

	#endregion


	#region structs

	[System.Serializable]
	public struct AudioCategory{
		public string m_name;
		public AudioChannelGroup[] m_audioChannelGroups;

		public AudioCategory(string categoryName, params string[] channelGroupNames){
			m_name = categoryName;
			m_audioChannelGroups = new AudioChannelGroup[channelGroupNames.Length];

			for (int i = 0; i < channelGroupNames.Length; i++){
				
				string[] channelNames = new string[Constants.VARIATIONS];
				for (int n = 0; n < Constants.VARIATIONS; n++){
					channelNames[n] = "Variation_" + (n+1);
				}

				m_audioChannelGroups[i] = new AudioChannelGroup(channelGroupNames[i], false, channelNames);
			}
		}

		public int GetRandomIndex(){
			return Random.Range(0, m_audioChannelGroups.Length);
		}
	}

	[System.Serializable]
	public struct AudioChannelGroup{
		public string m_name;

		public bool m_stack;

		public AudioChannel[] m_audioChannels;

		public AudioChannelGroup(string groupName, bool stack, params string[] channelNames){
			m_name = groupName;

			m_stack = stack;

			m_audioChannels = new AudioChannel[channelNames.Length];

			for (int i = 0; i < channelNames.Length; i++){
				m_audioChannels[i] = new AudioChannel(channelNames[i]);
			}
		}

		public int GetRandomIndex(){
			return Random.Range(0, m_audioChannels.Length);
		}

	}

	[System.Serializable]
	public struct AudioChannel{
		public string m_name;

		public AudioClip m_audioClip;

		public int m_loopLength;

		public AudioChannel(string name){
			m_name = name;
			m_audioClip = null;

			m_loopLength = 1;
		}

		public int Length(){
			return m_loopLength;
		}
	}

	#endregion

}
