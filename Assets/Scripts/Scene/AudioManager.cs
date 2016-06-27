using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Audio;

[ExecuteInEditMode, RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {

	public AudioMixer	m_audioMixer;

	public SoundSet		m_soundSet;

	public AudioClip 	m_masterAudioClip;
	public int 			m_bpm 					= 120;
	[Range(1,16)]
	public int			m_timeSignatureUpper	= 4;
	[Range(2,16)]
	public int 			m_timeSignatureLower	= 4;
	public int 			m_timeSampleOffset 		= 0;
	[Range(1,32)]
	public int 			m_unitsPerBeat			= 4;
	public float 		m_timeScale 			= 1.0f;

	public delegate void BeatDelegate(int num);
	public event BeatDelegate OnSubBeat;
	public event BeatDelegate OnBeat;
	public event BeatDelegate OnBar;

	public delegate void ControlDelegate();
	public event ControlDelegate OnPlay;
	public event ControlDelegate OnStop;

	// these are for internal calculations only
	private int m_currentSubBeat	= 1;
	private int m_currentBeat		= 1;
	private int m_currentBar		= 1;

	private AudioSource m_audioSource;		// masterAudioClip
	private AudioSource[] m_audioSources;	// every audio layer of the current track
	private AudioSourceSync[] m_audioSourcesSync;
	private float		m_audioSourceFrequency;
	private float 		m_beatTimer 		= 0;
	private float 		m_prevBeatTimer 	= 1.0f;
	private float		m_subBeatTimer		= 0;
	private float		m_prevSubBeatTimer	= 1.0f;

	private float 		m_masterTimer		= 0;

	// Use this for initialization
	void Awake () {
		// log error if there is no masteraudioclip
		if (m_masterAudioClip == null){
			Debug.LogError("Set MasterAudioClip in AudioManager object!");
			//Destroy(this);
		} else {
			if (m_audioMixer == null){
				Debug.LogError("No AudioMixer attached");
			}

			m_audioSource = GetComponent<AudioSource>();
			m_audioSource.clip = m_masterAudioClip;
			m_audioSourceFrequency = (float)m_audioSource.clip.frequency;

			// get references to every audioSource attached to this gameobject
			m_audioSources = GetComponentsInChildren<AudioSource>();
			m_audioSourcesSync = GetComponentsInChildren<AudioSourceSync>();

//			Debug.Log(GetTimePerBar());
//			Debug.Log(GetTimeAtBar(1));
//			Debug.Log(GetSamplesPerBar());

			Init();
		}
	}

	void OnDestroy(){
		OnSubBeat	= null;
		OnBeat		= null;
		OnBar		= null;
	}

	void Start(){
		//Play();
	}
	
	// Update is called once per frame
	void Update () {
		if (isPlaying()){
			// beat calculation
			//m_beatTimer = (((m_audioSource.timeSamples + m_timeSampleOffset) * (m_bpm * m_timeSignatureLower / 240.0f)) % (float)m_audioSourceFrequency) / 100000;

			m_subBeatTimer = (((m_audioSource.timeSamples + m_timeSampleOffset) * (m_bpm * m_timeSignatureLower / 240.0f * m_timeSignatureUpper)) % (float)m_audioSourceFrequency) / 100000;
			if (m_subBeatTimer < m_prevSubBeatTimer){
				SubBeat();
			}

//			if (m_beatTimer < m_prevBeatTimer){
//				Beat();
//			}
//			m_prevBeatTimer = m_beatTimer;

			m_prevSubBeatTimer = m_subBeatTimer;

			//SyncToAudioSource(m_audioSource, m_audioSources);
			SyncToAudioSource();

//			Debug.Log("timeSamples: " + m_audioSource.timeSamples);
		} else {
			// clamp lower time signatur
			m_timeSignatureLower = Mathf.ClosestPowerOfTwo(m_timeSignatureLower);

			// calculate new units per beat
			m_unitsPerBeat = (int)Mathf.Pow(Constants.UNITS_PER_BEAT, 2.0f) / m_timeSignatureLower;
		}
	}


	#region init

	private void Init(){
		if (m_soundSet == null){
			Debug.LogError("no soundset selected");
		} else {
			// load audio clips from soundset
			//Debug.Log(transform.childCount + " == " + m_soundSet.m_audioCategories.Length + " ?");
			//if (transform.childCount == m_soundSet.m_audioCategories.Length){
			for (int i = 0; i < m_soundSet.m_audioCategories.Length; i++){
				SoundSet.AudioCategory category = m_soundSet.m_audioCategories[i];

				GameObject gameObjectCategory = transform.GetChild(i).gameObject;

				//if (gameObjectCategory.transform.childCount == category.m_audioChannelGroups.Length){
					for (int j = 0; j < category.m_audioChannelGroups.Length; j++){
						SoundSet.AudioChannelGroup channelGroup = category.m_audioChannelGroups[j];

						GameObject gameObjectChannelGroup = gameObjectCategory.transform.GetChild(j).gameObject;

						//if (gameObjectChannelGroup.transform.childCount == channelGroup.m_audioChannels.Length){
							for (int k = 0; k < channelGroup.m_audioChannels.Length; k++){
								SoundSet.AudioChannel channel = channelGroup.m_audioChannels[k];

								GameObject gameObjectChannel = gameObjectChannelGroup.transform.GetChild(k).gameObject;

								gameObjectChannel.GetComponent<AudioSource>().clip = channel.m_audioClip;
							}

					}

			}
		} 
	}

	#endregion


	#region private functions

	void PlayAll(){
		m_audioSource.Play();
		foreach(AudioSourceSync source in m_audioSourcesSync){
			source.Play();
		}
	}

	void PauseAll(){
		m_audioSource.Pause();
		foreach(AudioSourceSync source in m_audioSourcesSync){
			source.Pause();
		}
	}

	void StopAll(){
		m_audioSource.Stop();
		foreach(AudioSourceSync source in m_audioSourcesSync){
			source.Stop();
		}
	}

	/**
	 * syncronises playback for every slave audiosource to the master audiosource
	 */
	// TODO: check if this needs to be applied every frame or rather on every beat (or bar) to reduce strain on performance
	void SyncToAudioSource(AudioSource master, AudioSource[] slaves){
		foreach (AudioSource slave in slaves){
			slave.timeSamples = master.timeSamples;
		}
	}

	void SyncToAudioSource(){
		foreach(AudioSourceSync slave in m_audioSourcesSync){
			slave.SyncToMaster(this);
		}
	}

	// gets called on every subbeat / movement unit
	void SubBeat(){
		//SyncToAudioSource();

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

		//SyncToAudioSource(m_audioSource, m_audioSources);
		//SyncToAudioSource();

//		Debug.Log(m_audioSource.time + " | Beat " + GetCurrentBeat());
	}

	// gets called on every full bar
	void Bar(){
		m_currentBar = GetCurrentBar();

		if (OnBar != null) OnBar(m_currentBar);

		//SyncToAudioSource(m_audioSource, m_audioSources);

		//Debug.Log("Bar");
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
		SyncToAudioSource(m_audioSource, m_audioSources);

		PlayAll();

		if (OnPlay != null){
			OnPlay();
		}
	}

	// pauses playback of all audiosources
	public void Pause(){
		PauseAll();
	}

	// stops (and resets) playback of all audiosources
	public void Stop(){
		StopAll();

		SetCurrentTime(0.0f);

		SetVolumeOfAllMixerGroups(-80.0f);

		if (OnStop != null){
			OnStop();
		}
	}

	public void GoToNextBar(){
		SetCurrentBar((GetCurrentBar() - 1) + 1);
	}

	public void GoToPrevBar(){
		SetCurrentBar((GetCurrentBar() - 1) - 1);
	}

	public void GoToNextBeat(){
		if (GetCurrentBeat() == m_timeSignatureUpper){
			GoToNextBar();
			SetCurrentBeat(1);
		} else {
			SetCurrentBeat(GetCurrentBeat() + 1);
		}
	}

	public void GoToPrevBeat(){
		if (GetCurrentBeat() == 1){
			GoToPrevBar();
			SetCurrentBeat(m_timeSignatureUpper);
		} else {
			SetCurrentBeat(GetCurrentBeat() - 1);
		}
	}

	#endregion

	#region public getter

	public int GetCurrentTimeSamples(){
		return m_audioSource.timeSamples;
	}

	public int GetSamplesPerBar(){
		return (int)(GetTimePerBar() * m_audioSourceFrequency);
	}

	public bool isPlaying(){
		return m_audioSource.isPlaying;
	}
		
	public int GetCurrentBar(){
		return (int)(((m_audioSource.timeSamples + m_timeSampleOffset) * (m_bpm * m_timeSignatureLower / 240.0f)) / (float)m_audioSourceFrequency) / m_timeSignatureUpper + 1;
	}

	public float GetCurrentBarTime(){
		return (((m_audioSource.timeSamples + m_timeSampleOffset) * (m_bpm * m_timeSignatureLower / 240.0f)) / (float)m_audioSourceFrequency) / m_timeSignatureUpper;
	}

	public float GetTimeAtBar(int _bar){
		return (m_timeSampleOffset / (float)m_audioSourceFrequency + m_timeSignatureUpper * _bar / (m_bpm * m_timeSignatureLower / 240.0f));
	}

	public int GetCurrentBeat(){
		return (int)(((m_audioSource.timeSamples + m_timeSampleOffset) * (m_bpm * m_timeSignatureLower / 240.0f)) / (float)m_audioSourceFrequency) % m_timeSignatureUpper + 1;
	}

	public float GetCurrentBeatTime(){
		return (((m_audioSource.timeSamples + m_timeSampleOffset) * (m_bpm * m_timeSignatureLower / 240.0f)) / (float)m_audioSourceFrequency) % m_timeSignatureUpper;
	}

	public int GetCurrentSubBeat(){
		return (int)(((m_audioSource.timeSamples + m_timeSampleOffset) * (m_bpm * m_timeSignatureLower / 240.0f * m_timeSignatureUpper)) / (float)m_audioSourceFrequency) % m_unitsPerBeat + 1;
	}

	public float GetCurrentSubBeatTime(){
		return (((m_audioSource.timeSamples + m_timeSampleOffset) * (m_bpm * m_timeSignatureLower / 240.0f * m_timeSignatureUpper)) / (float)m_audioSourceFrequency) % m_unitsPerBeat;
	}

	public float GetCurrentTime(){
		return m_audioSource.time;
	}

	public string GetCurrentTimeAsString(){
		return string.Format("{0:00}:{1:00}:{2:00}", (int)m_audioSource.time/60, (int)m_audioSource.time%60, (m_audioSource.time - (int)m_audioSource.time) * 100);
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
		return m_masterAudioClip.length;
		//return m_audioSource.clip.length;
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
		return GetTimeAtBar(1);
	}

	public float GetTimePerBeat(){
		return GetTimePerBar() / m_timeSignatureUpper;
	}

	public float GetTimePerSubBeat(){
		return GetTimePerBeat() / m_unitsPerBeat;
	}

	public float GetPositionX(int bar, int beat, int subBeat){
		return (float)( ((bar - 1) * m_unitsPerBeat * m_timeSignatureUpper) + ((beat - 1) * m_unitsPerBeat) + (subBeat - 1) );
	}

	#endregion

	#region public setter

	public void SetCurrentTime(float _time){
		m_audioSource.time = _time;
		foreach(AudioSource source in m_audioSources){
			source.time = _time;
		}

		SyncToAudioSource(m_audioSource, m_audioSources);
	}

	#endregion
}
