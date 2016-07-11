using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager instance;

	public bool m_debug = false;

	private AudioManager m_audioManager;
	private float m_horizontalSliderValue = 0.0f;

	[Header("Score")]
	public int patternComboMultiplier = 1;
	public Text scoreText;
	public Text highscoreText;
	private int patternCombo = 0;
	private int score		= 0;
	private int highscore	= 0;

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
		UpdateTextUI();
	}

	// look up new references
	void OnLevelWasLoaded(int sceneId){
		Debug.Log("Scene " + sceneId + " was loaded");
		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space) || MIDIInputManager.instance.GetPlayButtonDown()){
			if (!m_audioManager.IsPlaying()){
				m_audioManager.Play();
			} else {
				m_audioManager.Pause();
			}
		} 
		if (m_debug){
			if(Input.GetKeyDown(KeyCode.Backspace) || MIDIInputManager.instance.GetStopButtonDown()){
				m_audioManager.Stop();
			}

			if (Input.GetKeyDown(KeyCode.C)){
				m_audioManager.SetAllChannelsInActiveSoundSet(true);
			} else if (Input.GetKeyDown(KeyCode.M)){
				m_audioManager.SetAllChannelsInActiveSoundSet(false);
			}
		}

		if (Input.GetKeyDown(KeyCode.Escape)){
			Application.Quit();
		}

		if (Input.GetKeyDown(KeyCode.Return)){
			m_audioManager.QueueReset();
		}

		if (Input.GetKeyDown(KeyCode.N)){
			m_audioManager.QueueNextSoundSet();
		}
	}

	#region score

	public int GetScore(){
		return score;
	}

	public int GetHighscore(){
		return highscore;
	}

	public void AddScore(int value){
		score += value;

		if (score > highscore){
			highscore = score;
		}

		UpdateTextUI();
	}

	public void DecreaseScore(){
		if (patternCombo < 0){
			score += patternCombo;
		} else {
			score--;
		}

		if (score < 0){
			ResetScore();
		}

		UpdateTextUI();
	}

	public void ResetScore(){
		score = 0;

		UpdateTextUI();
	}

	public void IncreasePatternCombo(int patternSize){
		if (patternCombo < 0){
			patternCombo = 0;
		}

		patternCombo++;

		AddScore(patternCombo * patternComboMultiplier * patternSize);
	}

	public void DecreasePatternCombo(int patternSize){
		if (patternCombo > 0){
			patternCombo = 0;
		}

		patternCombo -= 1;
	}

	#endregion

	void UpdateTextUI(){
		if (scoreText != null){
			scoreText.text = "" + score;
		}
		if (highscoreText != null){
			highscoreText.text = "(" + highscore + ")";
		}
	}

	void OnGUI(){
		if (m_debug){
//			m_horizontalSliderValue = GUI.HorizontalSlider(new Rect(10, 10, Screen.width - 20, 20), m_audioManager.GetCurrentTime(), 0.0f, m_audioManager.GetClipLength());
//			if (m_horizontalSliderValue < m_audioManager.GetCurrentTime() || m_horizontalSliderValue > m_audioManager.GetCurrentTime()){
//				m_audioManager.SetCurrentTime(m_horizontalSliderValue);
//				m_horizontalSliderValue = m_audioManager.GetCurrentTime();
//			}

//			if (!m_audioManager.IsPlaying()){
//				if (GUI.Button(new Rect(10, 30, 60, 20), "PLAY")){
//					m_audioManager.Play();
//				}
//			} else {
//				if (GUI.Button(new Rect(10, 30, 60, 20), "PAUSE")){
//					m_audioManager.Pause();
//				}
//			}
//			if (GUI.Button(new Rect(70, 30, 60, 20), "STOP")){
//				m_audioManager.Stop();
//			}
//			if (GUI.Button(new Rect(130, 30, 60, 20), "RESET")){
//				m_audioManager.QueueReset();
//			}

			//GUI.Label(new Rect(10, 10, 100, 20), "SCORE: " + score + " (" + highscore + ")");
			GUI.Label(new Rect(10, 30, 100, 20), patternCombo + "x");

			GUI.Label(new Rect(10, 50, 100, 20), m_audioManager.GetCurrentAudioTimeAsString());
			GUI.Label(new Rect(70, 50, 100, 20), m_audioManager.GetCurrentBar() + " | " + m_audioManager.GetCurrentBeat() + " | " + m_audioManager.GetCurrentSubBeat());
			GUI.Label(new Rect(10, 70, 200, 20), m_audioManager.GetCurrentMasterTimeAsString());
		}
	}
}
