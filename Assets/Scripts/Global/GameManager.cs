using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager instance;

	public bool m_debug = false;

	private AudioManager m_audioManager;
	private float m_horizontalSliderValue = 0.0f;

	void Awake(){
		if (!instance){
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		} else {
			Destroy(this.gameObject);
		}

		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (m_audioManager == null){
			Debug.LogError("no AudioManager was found in this scene");
		}
	}

	// Use this for initialization
	void Start () {
		
	}

	// look up new references
	void OnLevelWasLoaded(int sceneId){
		Debug.Log("Scene " + sceneId + " was loaded");
		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (m_debug){
			if (Input.GetKeyDown(KeyCode.UpArrow) || MIDIInputManager.instance.GetForwardButtonDown()){
				m_audioManager.GoToNextBar();
			} else if (Input.GetKeyDown(KeyCode.DownArrow) || MIDIInputManager.instance.GetRewindButtonDown()){
				m_audioManager.GoToPrevBar();
			} else if (Input.GetKeyDown(KeyCode.RightArrow)){
				m_audioManager.GoToNextBeat();
			} else if (Input.GetKeyDown(KeyCode.LeftArrow)){
				m_audioManager.GoToPrevBeat();
			}
			if (Input.GetKeyDown(KeyCode.Space) || MIDIInputManager.instance.GetPlayButtonDown()){
				if (!m_audioManager.isPlaying()){
					m_audioManager.Play();
				} else {
					m_audioManager.Pause();
				}
			} else if(Input.GetKeyDown(KeyCode.Backspace) || MIDIInputManager.instance.GetStopButtonDown()){
				m_audioManager.Stop();
			}
		}

	}

	void OnGUI(){
		if (m_debug){
			m_horizontalSliderValue = GUI.HorizontalSlider(new Rect(10, 10, Screen.width - 20, 20), m_audioManager.GetCurrentTime(), 0.0f, m_audioManager.GetClipLength());
			if (m_horizontalSliderValue < m_audioManager.GetCurrentTime() || m_horizontalSliderValue > m_audioManager.GetCurrentTime()){
				m_audioManager.SetCurrentTime(m_horizontalSliderValue);
				m_horizontalSliderValue = m_audioManager.GetCurrentTime();
			}

			if (!m_audioManager.isPlaying()){
				if (GUI.Button(new Rect(10, 30, 60, 20), "PLAY")){
					m_audioManager.Play();
				}
			} else {
				if (GUI.Button(new Rect(10, 30, 60, 20), "PAUSE")){
					m_audioManager.Pause();
				}
			}
			if (GUI.Button(new Rect(70, 30, 60, 20), "STOP")){
				m_audioManager.Stop();
			}

			GUI.Label(new Rect(10, 50, 100, 20), m_audioManager.GetCurrentTimeAsString());
			GUI.Label(new Rect(70, 50, 100, 20), m_audioManager.GetCurrentBar() + " | " + m_audioManager.GetCurrentBeat() + " | " + m_audioManager.GetCurrentSubBeat());
			//GUI.Label(new Rect(10, 70, 200, 20), ""+m_audioManager.GetCurrentBarTime());
		}
	}
}
