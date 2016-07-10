using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Audio;

[ExecuteInEditMode, RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {

	public AudioMixerData audioMixerData;
	private AudioMixer	m_audioMixer;
	public SoundSet		activeSoundSet;
	public SoundSet[]	soundSets;
	public AudioClip 	m_masterAudioClip;

	[Header("Audio Settings")]
	public int 			m_bpm 					= 120;
	[Range(1,16)]
	public int			m_timeSignatureUpper	= 4;
	[Range(2,16)]
	public int 			m_timeSignatureLower	= 4;
	public int 			m_timeSampleOffset 		= 0;
	[Range(1,32)]
	[HideInInspector]
	public int 			m_unitsPerBeat			= 4;

	public delegate void BeatDelegate(int num);
	public event BeatDelegate OnSubBeat;
	public event BeatDelegate OnBeat;
	public event BeatDelegate OnBar;

	public delegate void ControlDelegate();
	public event ControlDelegate OnPlay;
	public event ControlDelegate OnStop;
	public event ControlDelegate OnReset;
	public event ControlDelegate OnReady;

	private bool resetQueued = false;
	private bool soundSetChangeQueue = false;

	public delegate void AudioGroupDelegate();
	public event AudioGroupDelegate OnAudioChannelChange;

	// these are for internal calculations only
	private int m_currentSubBeat	= 1;
	private int m_currentBeat		= 1;
	private int m_currentBar		= 0;

	private AudioSource metronomeAudioSource;		// masterAudioClip
	private AudioSourceSync[] m_audioSourcesSync;
	private AudioSource[,,] m_audioSourcesInChildren = new AudioSource[Constants.AUDIO_CATEGORIES, Constants.INSTRUMENT_GROUPS, Constants.VARIATIONS];
	private Dictionary<SoundSet, GameObject> soundSetRootDict;
	private Dictionary<SoundSet, AudioSource[,,]> audioSourceDict;
	private Dictionary<SoundSet, AudioSourceSync[]> audioSourceSyncDict;

	private float		m_audioSourceFrequency;
	private float		m_subBeatTimer		= 0;
	private float		m_prevSubBeatTimer	= 1.0f;

	private int			m_samplesPerBar		= 0;
	private float 		m_beatFactor		= 1;

	private float 		m_masterTimer		= 0;
	private float		m_prevAudioTime		= 0;

	// Use this for initialization
	void Awake () {
		metronomeAudioSource = GetComponent<AudioSource>();

		// log error if there is no masteraudioclip
		if (m_masterAudioClip == null){
			Debug.LogError("Set MasterAudioClip in AudioManager object!");
			//Destroy(this);
		} else {
			if (audioMixerData == null){
				Debug.LogError("No AudioMixerData attached");
			} else {
				m_audioMixer = audioMixerData.mainMixer;
			}

			metronomeAudioSource.clip = m_masterAudioClip;
			metronomeAudioSource.outputAudioMixerGroup = audioMixerData.metronomeTrack;
			m_audioSourceFrequency = (float)metronomeAudioSource.clip.frequency;
			metronomeAudioSource.loop = true;

			// get references to every audioSource attached to this gameobject
			m_audioSourcesSync = GetComponentsInChildren<AudioSourceSync>();

//			Debug.Log(GetTimePerBar());
//			Debug.Log(GetTimeAtBar(1));
//			Debug.Log(GetSamplesPerBar());

			//Init();
			if (Application.isPlaying){
				InitNew();
			}

//			float totalPossibleBars = (float.MaxValue / 2f) / (Constants.UNITS_PER_BEAT * m_timeSignatureUpper);
//			Debug.Log("Total possible bars: " + totalPossibleBars);
//			float timeUntilEndOfTheWorld = (float)totalPossibleBars * GetTimePerBar();
//			Debug.Log("Time until end of world: " + timeUntilEndOfTheWorld/3600f);
//			Debug.Log("Time to end of world: " + GetFloatAsTimeString(timeUntilEndOfTheWorld));
		}
	}

	void OnValidate(){
		UpdateBPM(m_bpm);
	}

	void OnDestroy(){
		metronomeAudioSource.Stop();

		OnSubBeat	= null;
		OnBeat		= null;
		OnBar		= null;

		OnAudioChannelChange = null;
		OnStop = null;
		OnPlay = null;
	}

	void Start(){
		if (Application.isPlaying){
			Play();
		}
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log(m_masterTimer + " | " + m_currentSamples);
		if (IsPlaying()){
			// add audio delta to master timer
			if (metronomeAudioSource.time - m_prevAudioTime < 0){
				m_masterTimer += (metronomeAudioSource.clip.length - m_prevAudioTime) + metronomeAudioSource.time;
			} else {
				m_masterTimer += (metronomeAudioSource.time - m_prevAudioTime);
			}


			// beat calculation
			m_subBeatTimer = (((metronomeAudioSource.timeSamples + m_timeSampleOffset) * (m_bpm * m_timeSignatureLower / 240.0f * m_timeSignatureUpper)) % (float)m_audioSourceFrequency) / 100000;
			if (m_subBeatTimer < m_prevSubBeatTimer){
				SubBeat();
			}

			// set memory variables
			m_prevSubBeatTimer = m_subBeatTimer;
			m_prevAudioTime = metronomeAudioSource.time;


		} else {
			// clamp lower time signature
			m_timeSignatureLower = Mathf.ClosestPowerOfTwo(m_timeSignatureLower);

			// calculate new units per beat
			//m_unitsPerBeat = (int)Mathf.Pow(Constants.UNITS_PER_BEAT, 2.0f) / m_timeSignatureLower;
			m_unitsPerBeat = Constants.UNITS_PER_BEAT;
		}
	}


	#region beat functions

	// gets called on every subbeat / movement unit
	void SubBeat(){
//		SyncToAudioSource();

		m_currentSubBeat = GetCurrentSubBeat();

		if (OnSubBeat != null) OnSubBeat(m_currentSubBeat);

		if (m_currentSubBeat == 1){
			Beat();
		}

		//		Debug.Log(m_audioSource.time + " | SubBeat " + GetCurrentSubBeat());
	}

	// gets called on every beat
	void Beat(){
		m_currentBeat = GetCurrentBeat();

		if (OnBeat != null) OnBeat(m_currentBeat);

		if (m_currentBeat == 1){
			Bar();
		}

		SyncToAudioSource();
	}

	// gets called on every full bar
	void Bar(){
		if (resetQueued){
			Reset();
		} else if (soundSetChangeQueue){
			NextSoundSet();
			soundSetChangeQueue = false;
		}

		m_currentBar = GetCurrentBar();

		if (OnBar != null) OnBar(m_currentBar);

//		Debug.Log("Bar " + m_currentBar);
	}


	#endregion


	#region init

	private void Init(){
		if (activeSoundSet == null){
			Debug.LogError("no soundset selected");
		} else {
			UpdateBPM(activeSoundSet.bpm);

			// reset all sources
			foreach(AudioSource source in GetComponentsInChildren<AudioSource>()){
				if (source != metronomeAudioSource){
					source.clip = null;
					source.mute = true;
				}
			}

			// load audio clips from soundset
			for (int i = 0; i < Constants.AUDIO_CATEGORIES; i++){
				if (i < activeSoundSet.m_audioCategories.Length){
					SoundSet.AudioCategory category = activeSoundSet.m_audioCategories[i];
					GameObject gameObjectCategory = transform.GetChild(i).gameObject;

					for (int j = 0; j < Constants.INSTRUMENT_GROUPS; j++){
						if (j < category.m_audioChannelGroups.Length){
							SoundSet.AudioChannelGroup channelGroup = category.m_audioChannelGroups[j];
							GameObject gameObjectChannelGroup = gameObjectCategory.transform.GetChild(j).gameObject;

							for (int k = 0; k < Constants.VARIATIONS; k++){
								if (k < channelGroup.m_audioChannels.Length){
									GameObject gameObjectChannel = gameObjectChannelGroup.transform.GetChild(k).gameObject;
									m_audioSourcesInChildren[i, j, k] = gameObjectChannel.GetComponent<AudioSource>();
									m_audioSourcesInChildren[i, j, k].mute = true;
									SoundSet.AudioChannel channel = channelGroup.m_audioChannels[k];
									m_audioSourcesInChildren[i, j, k].clip = channel.m_audioClip;
								}
							}
						}

					}
				}
			}
		} 
	}

	void InitNew(){
		if (soundSets == null || soundSets.Length < 1){
			Debug.LogError("There are no soundsets");
			return;
		}
			
//		while (transform.childCount > 0){
//			Destroy(transform.GetChild(0));
//		}

		soundSetRootDict = new Dictionary<SoundSet, GameObject>();
		audioSourceDict = new Dictionary<SoundSet, AudioSource[,,]>();
		audioSourceSyncDict = new Dictionary<SoundSet, AudioSourceSync[]>();
		int audioSourceSyncIndex = 0;

		// instantiate children and load soundset audio
		for (int s = 0; s < soundSets.Length; s++){
			GameObject root = new GameObject(soundSets[s].name);
			root.transform.parent = this.transform;

			AudioSource[,,] audioSources = new AudioSource[Constants.AUDIO_CATEGORIES, Constants.INSTRUMENT_GROUPS, Constants.VARIATIONS];
			AudioSourceSync[] audioSourcesSync = new AudioSourceSync[Constants.AUDIO_CATEGORIES * Constants.INSTRUMENT_GROUPS * Constants.VARIATIONS];

			for (int c = 0; c < Constants.AUDIO_CATEGORIES; c++){
				Transform category = new GameObject(soundSets[s].m_audioCategories[c].m_name).transform;
				category.parent = root.transform;

				for (int i = 0; i < Constants.INSTRUMENT_GROUPS; i++){
					Transform instrument = new GameObject(soundSets[s].m_audioCategories[c].m_audioChannelGroups[i].m_name).transform;
					instrument.parent = category;

					for (int v = 0; v < Constants.VARIATIONS; v++){
						GameObject variation = new GameObject(soundSets[s].m_audioCategories[c].m_audioChannelGroups[i].m_audioChannels[v].m_name);
						variation.transform.parent = instrument;

						AudioSource audioSource = variation.AddComponent<AudioSource>();
						audioSource.clip = soundSets[s].m_audioCategories[c].m_audioChannelGroups[i].m_audioChannels[v].m_audioClip;
						audioSource.playOnAwake = false;
						audioSource.mute = true;
						audioSource.outputAudioMixerGroup = audioMixerData.GetAudioMixerGroup(c, i, v);
						AudioSourceSync audioSourceSync = variation.AddComponent<AudioSourceSync>();
						audioSourceSync.SetLoop(true);


						audioSources[c,i,v] = audioSource;
						audioSourcesSync[audioSourceSyncIndex] = audioSourceSync;
						audioSourceSyncIndex++;
					}
				}
			}

			if (s > 0){
				root.SetActive(false);
			}

			soundSetRootDict.Add(soundSets[s], root);
			audioSourceDict.Add(soundSets[s], audioSources);
			audioSourceSyncDict.Add(soundSets[s], audioSourcesSync);

			audioSourceSyncIndex = 0;
		}

		// set active sound set;
		activeSoundSet = soundSets[0];
		TransitionToAudioSnapShot(activeSoundSet, 0);
		UpdateBPM(activeSoundSet.bpm);

	}

	#endregion


	#region sound set functions

	public void QueueNextSoundSet(){
		soundSetChangeQueue = true;
	}

	void NextSoundSet(){
		int currentIndex = -1;
		for (int i = 0; i < soundSets.Length; i++){
			if (soundSets[i] == activeSoundSet){
				currentIndex = i;
			}
		}

		if (currentIndex < 0){
			Debug.LogWarning("active soundset not found");
			return;
		}

		int nextIndex = (currentIndex + 1) % soundSets.Length;
		//Debug.Log(nextIndex);

		ChangeSoundSet(nextIndex);
	}

	void ChangeSoundSet(int index){
		if (index >= soundSets.Length){
			return;
		}

		// stop old soundset
		StopActiveSoundSet();

		// mute old soundset
		SetAllChannelsInActiveSoundSet(false);

		// deactivate old gameobjects
		DeactivateActiveSoundSetRoot();

		// activate new gameobjects
		ActivateSoundSetRoot(index);

		// set new soundset as the active soundset
		activeSoundSet = soundSets[index];

		// transition to snapshot
		TransitionToAudioSnapShot(activeSoundSet, 0);

		// play new soundset
		PlayAllFromActiveSoundSet();

	}

	void TransitionToAudioSnapShot(SoundSet soundSet, float time){
		if (soundSet.audioMixerSnapshot != null){
			soundSet.audioMixerSnapshot.TransitionTo(time);
		} else {
			audioMixerData.standardSnapshot.TransitionTo(time);
		}
	}

	void StopActiveSoundSet(){
		for (int i = 0; i < audioSourceSyncDict[activeSoundSet].Length; i++){
			audioSourceSyncDict[activeSoundSet][i].Stop();
		}
	}

	void DeactivateActiveSoundSetRoot(){
		soundSetRootDict[activeSoundSet].SetActive(false);
	}

	void ActivateSoundSetRoot(int index){
		soundSetRootDict[soundSets[index]].SetActive(true);
	}

	#endregion


	#region private functions

	void PlayAll(){
		metronomeAudioSource.Play();
		PlayAllFromActiveSoundSet();
	}

	void PlayAllFromActiveSoundSet(){
		
//		foreach(AudioSourceSync source in m_audioSourcesSync){
//			source.Play();
//		}

		for (int i = 0; i < audioSourceSyncDict[activeSoundSet].Length; i++){
			audioSourceSyncDict[activeSoundSet][i].Play();
		}
	}

	void PauseActiveSoundSet(){
		metronomeAudioSource.Pause();
//		foreach(AudioSourceSync source in m_audioSourcesSync){
//			source.Pause();
//		}

		for (int i = 0; i < audioSourceSyncDict[activeSoundSet].Length; i++){
			audioSourceSyncDict[activeSoundSet][i].Pause();
		}
	}

	void StopAll(){
		metronomeAudioSource.Stop();
//		foreach(AudioSourceSync source in m_audioSourcesSync){
//			source.Stop();
//		}

		foreach (SoundSet soundSet in audioSourceSyncDict.Keys){
			for (int i = 0; i < audioSourceSyncDict[activeSoundSet].Length; i++){
				audioSourceSyncDict[soundSet][i].Stop();
			}
		}
	}

	void UpdateBPM(int newBpm){
		m_bpm = newBpm;

		if (metronomeAudioSource != null){
			m_samplesPerBar = Mathf.RoundToInt(GetTimePerBar() * metronomeAudioSource.clip.frequency);
		}

		m_beatFactor = (m_bpm * m_timeSignatureLower / 240.0f) / m_timeSignatureUpper;

//		Debug.Log("BPM: " + m_bpm);
//		Debug.Log("SamplesPerBar: " + m_samplesPerBar);
//		Debug.Log("BeatFactor: " + m_beatFactor);
	}

	/**
	 * syncronises playback for every slave audiosource to the master audiosource
	 */

	void SyncToAudioSource(){
		if (Application.isPlaying){
//			foreach(AudioSourceSync slave in m_audioSourcesSync){
//				slave.SyncToMaster(this);
//			}
//			foreach(AudioSourceSync slave in audioSourceSyncDict[activeSoundSet]){
//				slave.SyncToMaster(this);
//			}
			for (int s = 0; s < audioSourceSyncDict[activeSoundSet].Length; s++){
				audioSourceSyncDict[activeSoundSet][s].SyncToMaster(this);
			}
		}
	}

	void SyncToAudioSource(AudioSource master, AudioSource[] slaves){
		foreach (AudioSource slave in slaves){
			slave.timeSamples = master.timeSamples;
		}
	}

	void SetCurrentBar(int _bar){
		float targetTime = GetTimeAtBar(_bar);

		//Debug.Log("targetBar: " + _bar + " / targetTime: " + targetTime);

		if (targetTime > GetClipLength()){
			Stop();
			return;
		} else if (targetTime <= 0){
			targetTime = 0.0f;
		}

		SetCurrentTime(targetTime);
	}

	void SetCurrentBeat(int _beat){
		float targetTime = GetTimeAtBar(GetCurrentBar() - 1) + GetTimeAtBar(1) * (_beat - 1)/m_timeSignatureUpper;

		//Debug.Log("targetBeat: " + _beat + " / targetTime: " + targetTime);

		if (targetTime > GetClipLength()){
			Stop();
			return;
		} else if (targetTime <= 0){
			targetTime = 0.0f;
		}

		SetCurrentTime(targetTime);
	}

	void SetVolumeOfAllMixerGroups(float _volume){
		foreach(Enums.AudioMixerExposedParams mixerGroupParam in Enum.GetValues(typeof(Enums.AudioMixerExposedParams))){
			m_audioMixer.SetFloat(mixerGroupParam.ToString(), _volume);
		}
	}

	#endregion


	#region public functions

	// starts playback of all audiosources
	public void Play(){
		TransitionToAudioSnapShot(activeSoundSet, 0);

		PlayAll();

		SyncToAudioSource();

		if (OnPlay != null){
			OnPlay();
		}
	}

	// pauses playback of all audiosources
	public void Pause(){
		PauseActiveSoundSet();
	}

	// stops (and resets) playback of all audiosources
	public void Stop(){
		SetCurrentTime(0);

		StopAll();

		//SetVolumeOfAllMixerGroups(-80.0f);
		SetAllChannelsInActiveSoundSet(false);

		m_currentBar = 0;

		if (OnStop != null){
			OnStop();
		}
	}

	public void QueueReset(){
		resetQueued = true;
	}

	void Reset(){
		resetQueued = false;
		SetAllChannelsInActiveSoundSet(false);
		SetCurrentTime(0);

		if (OnReset != null){
			OnReset();
		}
	}
//
//	public void GoToNextBar(){
//		SetCurrentBar((GetCurrentBar() - 1) + 1);
//	}
//
//	public void GoToPrevBar(){
//		SetCurrentBar((GetCurrentBar() - 1) - 1);
//	}
//
//	public void GoToNextBeat(){
//		if (GetCurrentBeat() == m_timeSignatureUpper){
//			GoToNextBar();
//			SetCurrentBeat(1);
//		} else {
//			SetCurrentBeat(GetCurrentBeat() + 1);
//		}
//	}
//
//	public void GoToPrevBeat(){
//		if (GetCurrentBeat() == 1){
//			GoToPrevBar();
//			SetCurrentBeat(m_timeSignatureUpper);
//		} else {
//			SetCurrentBeat(GetCurrentBeat() - 1);
//		}
//	}
//
	#endregion


	#region audio channel controll

	public void OnAudioTrigger(bool success, int category, int instrument, int variation){
		//Debug.Log(success + " " + category + " " + instrument + " " + variation);
//		if (m_audioSourcesInChildren[category, instrument, variation] == null){
//			return;
//		}
//		if (success){
//			// enable audio
//			if (!CanStack(category, instrument)){
//				for (int i = 0; i < m_audioSourcesInChildren.GetLength(2); i++){
//					if (m_audioSourcesInChildren[category, instrument, i] != null){
//						m_audioSourcesInChildren[category, instrument, i].mute = (variation == i) ? false : true;
//					}
//				}
//			} else {
//				m_audioSourcesInChildren[category, instrument, variation].mute = false;
//			}
//
//		} else {
//			
//			// disable audio
//			if (!m_audioSourcesInChildren[category, instrument, variation].mute){
//				m_audioSourcesInChildren[category, instrument, variation].mute = true;
//			} else {
//				
//				if (CanStack(category, instrument)){
//					// mute something else from that group
//					int[] indices = GetIndicesOfMutedVarations(false, category, instrument);
//					if (indices.Length > 0){
//						m_audioSourcesInChildren[category, instrument, UnityEngine.Random.Range(0, indices.Length)].mute = true;
//					}
//				}
//			}
//			
//		}

		if (audioSourceDict[activeSoundSet][category, instrument, variation] == null){
			return;
		}
		if (success){
			// enable audio
			if (!CanStack(category, instrument)){
				for (int v = 0; v < audioSourceDict[activeSoundSet].GetLength(2); v++){
					if (audioSourceDict[activeSoundSet][category, instrument, v] != null){
						audioSourceDict[activeSoundSet][category, instrument, v].mute = (variation == v) ? false : true;
					}
				}
			} else {
				audioSourceDict[activeSoundSet][category, instrument, variation].mute = false;
			}

		} else {
			int variationToMute = -1;

			Debug.Log("disabling audio...");
			// disable audio of same variation if it is already active
			if (!audioSourceDict[activeSoundSet][category, instrument, variation].mute){
				Debug.Log("channel was playing. disabling this motherfucker!");
				audioSourceDict[activeSoundSet][category, instrument, variation].mute = true;
			} else {
				Debug.Log("channel was not playing.");
				if (CanStack(category, instrument)){
					// mute something else from that group
					Debug.Log("instrument can stack");
					int[] indices = GetIndicesOfMutedVarations(false, category, instrument);
					if (indices.Length > 0){
						Debug.Log("there were other variations playing");
						variationToMute = indices[UnityEngine.Random.Range(0, indices.Length)];
						audioSourceDict[activeSoundSet][category, instrument, variationToMute].mute = true;
						Debug.Log("muted variation " + variationToMute);
						return;
					}
				}
				Debug.Log("no other variation was is playing");
				// there is no stack or other variation playing; look for other instruments in this category
				int indexOfInstrumentToMute = GetInstrumentWithMostMuted(false, category);
				Debug.Log("muting a variation of instrument " + indexOfInstrumentToMute);
				int[] indicesOfVariations = GetIndicesOfMutedVarations(false, category, indexOfInstrumentToMute);
				Debug.Log("currently " + indicesOfVariations.Length + " variations playing.");
				if (indicesOfVariations.Length > 0){
					variationToMute = indicesOfVariations[UnityEngine.Random.Range(0, indicesOfVariations.Length)];
					audioSourceDict[activeSoundSet][category, indexOfInstrumentToMute, variationToMute].mute = true;
					Debug.Log("muted variation " + variationToMute);
					return;
				} 
			}

		}
	}
		
	int[] GetIndicesOfMutedVarations(bool muted, int category, int instrument){
		List<int> intList = new List<int>();

		//Debug.Log(muted + " " + category + " " + instrument);

//		for (int i = 0; i < m_audioSourcesInChildren.GetLength(2); i++){
//			if (m_audioSourcesInChildren[category, instrument, i] != null){
//				if(m_audioSourcesInChildren[category, instrument, i].mute == muted){
//					intList.Add(i);
//				}	
//			}
//		}

		for (int i = 0; i < audioSourceDict[activeSoundSet].GetLength(2); i++){
			if (audioSourceDict[activeSoundSet][category, instrument, i] != null){
				if(audioSourceDict[activeSoundSet][category, instrument, i].mute == muted){
					intList.Add(i);
				}	
			}
		}

		return intList.ToArray();
	}

	int[] GetNumberOfMutedVariationsInCategory(bool muted, int category){
//		int[] returnArray = new int[m_audioSourcesInChildren.GetLength(1)];
		int[] returnArray = new int[audioSourceDict[activeSoundSet].GetLength(1)];

		for (int i = 0; i < returnArray.Length; i++){
			returnArray[i] = GetIndicesOfMutedVarations(muted, category, i).Length;
		}

		return returnArray;
	}

	int GetInstrumentWithMostMuted(bool muted, int category){
		int[] numberOfMutedVariationsInCategory = GetNumberOfMutedVariationsInCategory(muted, category);

		List<int> indicesOfInstruments = new List<int>();

		for (int v = Constants.VARIATIONS; v >= 0; v--){
			for (int i = 0; i < numberOfMutedVariationsInCategory.Length; i++){
				if (numberOfMutedVariationsInCategory[i] == v){
					indicesOfInstruments.Add(i);
				}

				if (indicesOfInstruments.Count > 0){
					return indicesOfInstruments[UnityEngine.Random.Range(0, indicesOfInstruments.Count)];
				}
			}
		}

		return -1;
	}

	/**
	 * Returns the index of the instrument of category with the most muted variations
	 */
	public int GetRandomInstrument(int category){
//		return UnityEngine.Random.Range(0, m_audioSourcesInChildren.GetLength(1));
		return UnityEngine.Random.Range(0, audioSourceDict[activeSoundSet].GetLength(1));

		int[] mostMuted = GetNumberOfMutedVariationsInCategory(true, category);
		int mostMutedIndex = 0;
		int mostMutedValue = mostMuted[0];

		if (mostMuted.Length < 2){
			return 0;
		}

		for (int i = 1; i < mostMuted.Length; i++){
			//Debug.Log(i + ": " + mostMuted[i-1]);
			if (mostMuted[i] > mostMutedValue){
				mostMutedIndex = i;
				mostMutedValue = mostMuted[i];
			}
		}
		//Debug.Log(mostMutedIndex);

		return mostMutedIndex;
	}

	/**
	 * returns a random variation of category, instrument that is muted
	 */
	public int GetRandomVariation(int category, int instrument){
		int[] mutedVariations = GetIndicesOfMutedVarations(true, category, instrument);
		if (mutedVariations.Length < 1) return 0;
		return mutedVariations[UnityEngine.Random.Range(0, mutedVariations.Length)];
	}

	public void SetChannelActive(bool active, int category, int instrument, int variation){
//		if (category >= 0 && category < m_audioSourcesInChildren.GetLength(0) &&
//			instrument >= 0 && instrument < m_audioSourcesInChildren.GetLength(1) &&
//			variation >= 0 && variation < m_audioSourcesInChildren.GetLength(2)){
//			if (m_audioSourcesInChildren[category, instrument, variation] != null){
//				m_audioSourcesInChildren[category, instrument, variation].mute = !active;
//				//Debug.Log("SetChannelActive: " + active + ", " + category + ", " + instrument + ", " + variation);
//			}
//		}
		if (category >= 0 && category < audioSourceDict[activeSoundSet].GetLength(0) &&
			instrument >= 0 && instrument < audioSourceDict[activeSoundSet].GetLength(1) &&
			variation >= 0 && variation < audioSourceDict[activeSoundSet].GetLength(2)){
			if (audioSourceDict[activeSoundSet][category, instrument, variation] != null){
				audioSourceDict[activeSoundSet][category, instrument, variation].mute = !active;
				//Debug.Log("SetChannelActive: " + active + ", " + category + ", " + instrument + ", " + variation);
			}
		}
	}

	public void SetAllChannelsInActiveSoundSet(bool active){
//		foreach (AudioSource source in m_audioSourcesInChildren){
//			if (source != null){
//				source.mute = !active;
//			}
//		}
		foreach (AudioSource source in audioSourceDict[activeSoundSet]){
			if (source != null){
				source.mute = !active;
			}
		}
	}

	public bool IsChannelActive(int category, int instrument, int variation){
//		if (m_audioSourcesInChildren[category, instrument, variation] != null){
//			return !m_audioSourcesInChildren[category, instrument, variation].mute;
//		}
		if (audioSourceDict[activeSoundSet][category, instrument, variation] != null){
			return !audioSourceDict[activeSoundSet][category, instrument, variation].mute;
		}
		return false;	
	}


	#endregion


	#region soundset functions

	public int GetMutedVariation(int category, int instrument){
		return 0;
	}

	public bool CanStack(int category, int instrument){
		return activeSoundSet.m_audioCategories[category].m_audioChannelGroups[instrument].m_stack;
	}

	#endregion


	#region public audio getter
		
	public int GetCurrentAudioTimeSamples(){
		return metronomeAudioSource.timeSamples + m_timeSampleOffset;
	}

	public int GetSamplesPerBar(){
		return m_samplesPerBar;
		return Mathf.RoundToInt(GetTimePerBar() * metronomeAudioSource.clip.frequency);
	}

	public bool IsPlaying(){
		return metronomeAudioSource.isPlaying;
	}
		
	public int GetCurrentBar(){
		//return m_currentBar;
		return Mathf.FloorToInt(GetCurrentMasterTime() * m_beatFactor) + 1;
		return (int)((GetCurrentAudioTimeSamples() * (m_bpm * m_timeSignatureLower / 240.0f)) / (float)m_audioSourceFrequency) / m_timeSignatureUpper + 1;
	}

	public float GetCurrentBarTime(){
		return GetCurrentMasterTime() * m_beatFactor;
		return ((GetCurrentAudioTimeSamples() * (m_bpm * m_timeSignatureLower / 240.0f)) / (float)m_audioSourceFrequency) / m_timeSignatureUpper;
	}

	public float GetTimeAtBar(int _bar){
		return GetTimePerBar() * (_bar - 1);
		return (m_timeSampleOffset / (float)m_audioSourceFrequency + m_timeSignatureUpper * _bar / (m_bpm * m_timeSignatureLower / 240.0f));
	}

	public int GetCurrentBeat(){
		return Mathf.FloorToInt(GetCurrentMasterTime() * (m_bpm * m_timeSignatureLower / 240.0f)) % m_timeSignatureUpper + 1;
		return (int)((GetCurrentAudioTimeSamples() * (m_bpm * m_timeSignatureLower / 240.0f)) / (float)m_audioSourceFrequency) % m_timeSignatureUpper + 1;
	}

	public float GetCurrentBeatTime(){
		return ((GetCurrentMasterTimeSamples() * (m_bpm * m_timeSignatureLower / 240.0f)) / (float)m_audioSourceFrequency) % m_timeSignatureUpper;
		return ((GetCurrentAudioTimeSamples() * (m_bpm * m_timeSignatureLower / 240.0f)) / (float)m_audioSourceFrequency) % m_timeSignatureUpper;
	}

	public int GetCurrentSubBeat(){
		return Mathf.FloorToInt((GetCurrentMasterTime() * (m_bpm * m_timeSignatureLower / 240.0f * m_timeSignatureUpper))) % m_unitsPerBeat + 1;
		return (int)((GetCurrentAudioTimeSamples() * (m_bpm * m_timeSignatureLower / 240.0f * m_timeSignatureUpper)) / (float)m_audioSourceFrequency) % m_unitsPerBeat + 1;
	}

	public float GetCurrentSubBeatTime(){
		return m_subBeatTimer;
		return ((GetCurrentAudioTimeSamples() * (m_bpm * m_timeSignatureLower / 240.0f * m_timeSignatureUpper)) / (float)m_audioSourceFrequency) % m_unitsPerBeat;
	}

	public float GetCurrentMasterTime(){
		return m_masterTimer;
	}

	public float GetCurrentAudioTime(){
		return metronomeAudioSource.time;
	}

	public string GetCurrentMasterTimeAsString(){
		return string.Format("{0:00}:{1:00}:{2:00}", (int)GetCurrentMasterTime()/60, (int)GetCurrentMasterTime()%60, (GetCurrentMasterTime() - (int)GetCurrentMasterTime()) * 100);
	}

	public string GetCurrentAudioTimeAsString(){
		return string.Format("{0:00}:{1:00}:{2:00}", (int)GetCurrentAudioTime()/60, (int)GetCurrentAudioTime()%60, (GetCurrentAudioTime() - (int)GetCurrentAudioTime()) * 100);
	}

	public static string GetFloatAsTimeString(float time){
		return string.Format("{0:00}:{1:00}:{2:00}", (int)time/60, (int)time%60, (time - (int)time) * 100);
	}

	public int GetTimeSignatureUpper(){
		return m_timeSignatureUpper;
	}

	public int GetTimeSignatureLower(){
		return m_timeSignatureLower;
	}

	public string GetCurrentTimeSignatureAsString(){
		return m_timeSignatureUpper+"/"+m_timeSignatureLower;
	}
		
	public float GetClipLength(){
		return metronomeAudioSource.clip.length;
	}

	public int GetUnitsPerBeat(){
		return m_unitsPerBeat;
	}

	public int GetTotalBars(){
		return (int)(GetClipLength() / 60.0f * (float)m_bpm / (float)m_timeSignatureUpper);
	}

	public AudioMixer GetAudioMixer(){
		return m_audioMixer;
	}

	public float GetTimePerBar(){
		return GetTimePerBeat() * m_timeSignatureUpper;
		return GetTimeAtBar(1);
	}

	public float GetTimePerBeat(){
		return 60.0f /  m_bpm;
		return GetTimePerBar() / m_timeSignatureUpper;
	}

	public float GetTimePerSubBeat(){
		return GetTimePerBeat() / m_unitsPerBeat;
	}

	public float GetPositionX(int bar, int beat, int subBeat){
		return (float)( ((bar - 1) * m_unitsPerBeat * m_timeSignatureUpper) + ((beat - 1) * m_unitsPerBeat) + (subBeat - 1) );
	}

	#endregion

	#region public master getter

	public int GetCurrentMasterTimeSamples(){
		return (int)(m_masterTimer * metronomeAudioSource.clip.frequency + m_timeSampleOffset);
	}

	#endregion

	#region public setter

	public void SetCurrentTime(float _time){
		m_masterTimer = _time;
		metronomeAudioSource.time = _time % metronomeAudioSource.clip.length;
		m_prevAudioTime = metronomeAudioSource.time;
		SyncToAudioSource();
	}

	#endregion
}
