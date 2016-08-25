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
	public Text comboText;
	private int patternCombo = 0;
	private int score		= 0;
	private int highscore	= 0;

	[Header("IdleScreen")]
	public Image idleScreenImage;
	public float fadeInTime		= .3f;
	public float fadeOutTime	= .6f;
	public float screenOnStartupTime = 3.0f;
	private bool checkForIdle = false;
	private bool isFading = false;

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
		ShowIdleScreen();
		Invoke("HideIdleScreen", screenOnStartupTime);
	}

	// look up new references
	void OnLevelWasLoaded(int sceneId){
		Debug.Log("Scene " + sceneId + " was loaded");
		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
	}
	
	// Update is called once per frame
	void Update () {
//		if (Input.GetKeyDown(KeyCode.Space) || MIDIInputManager.instance.GetPlayButtonDown()){
//			if (!m_audioManager.IsPlaying()){
//				m_audioManager.Play();
//			} else {
//				m_audioManager.Pause();
//			}
//		} 
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

		if (checkForIdle){
			if (MIDIInputManager.instance.IsIdle()){
				ShowIdleScreen();
			} else {
				HideIdleScreen();
			}
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

	public int GetCombo(){
		return patternCombo;
	}

	#endregion


	#region idle screen

	void ShowIdleScreen(){
		if (idleScreenImage.color.a < 1.0f){
			Debug.Log("Show Idle Screen");
			StartCoroutine(FadeImageCoroutine(idleScreenImage, fadeInTime, 1.0f));
		}
	}

	void HideIdleScreen(){
		if (idleScreenImage.color.a > 0){
			Debug.Log("Hide Idle Screen");
			StartCoroutine(FadeImageCoroutine(idleScreenImage, fadeOutTime, 0));
		}

		checkForIdle = true;
	}

	private IEnumerator FadeImageCoroutine(Image image, float fadeTime, float toOpacity){
		while (isFading){
			yield return null;
		}

		isFading = true;

		Color fromColor = image.color;
		Color toColor = fromColor;
		toColor.a = toOpacity;

		for (float t = 0; t < fadeTime; t += Time.deltaTime){
			image.color = Color.Lerp(fromColor, toColor, t/fadeTime);
			yield return null;
		}

		image.color = toColor;

		isFading = false;
	}

	#endregion


	void UpdateTextUI(){
		if (scoreText != null){
			scoreText.text = "" + score;
		}
		if (highscoreText != null){
			highscoreText.text = "(" + highscore + ")";
		}
		if (comboText != null){
			comboText.text = (patternCombo > 1) ? patternCombo + "x" : "";
		}
	}

	void OnGUI(){
		if (m_debug){
			GUI.Label(new Rect(10, 50, 100, 20), m_audioManager.GetCurrentAudioTimeAsString());
			GUI.Label(new Rect(70, 50, 100, 20), m_audioManager.GetCurrentBar() + " | " + m_audioManager.GetCurrentBeat() + " | " + m_audioManager.GetCurrentSubBeat());
			GUI.Label(new Rect(10, 70, 200, 20), m_audioManager.GetCurrentMasterTimeAsString());
		}
	}
}
