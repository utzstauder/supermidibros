using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[CreateAssetMenu (menuName = "Data/SoundSet", fileName = "SoundSet", order = 100)]
public class SoundSet : ScriptableObject {

	public AudioChannelGroup[] m_audioChannelGroups;

	public class AudioChannelGroup{
		public string m_name;

		public AudioChannel[] m_audioChannels;

		public AudioChannelGroup(){
			
		}

		public AudioChannelGroup(string name){
			m_name = name;
		}

	}

	public struct AudioChannel{
		public string m_name;

		public AudioChannel(string name){
			m_name = name;
		}
	}

}
