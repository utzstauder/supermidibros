using UnityEngine;
using System.Collections;
using System.IO;

public class ScreenCapture : MonoBehaviour {

	public bool m_captureVideoInPlaymode	= false;
	public bool m_lockFrameRate				= false;
	public int m_frameLock					= 30;
	public string m_videoframeSubfolder		= "capturedframes";
	public string m_screenshotsSubfolder	= "screenshots";
	public string m_screenShotPrefix		= "Screenshot";
	public KeyCode m_screenShotInput		= KeyCode.Return;

	private bool m_isRecording = false;
	private int m_frameCount = 0;

	private AudioManager m_audioManager;

	// Use this for initialization
	void Awake () {
		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (m_audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {
			m_audioManager.OnPlay += StartCapture;
			m_audioManager.OnStop += StopCapture;
		}

		if (!Directory.Exists(m_videoframeSubfolder)){
			Directory.CreateDirectory(m_videoframeSubfolder);
		}

		if (!Directory.Exists(m_screenshotsSubfolder)){
			Directory.CreateDirectory(m_screenshotsSubfolder);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (m_captureVideoInPlaymode && m_isRecording){
			CaptureFrame();
		} else if (Input.GetKeyDown(m_screenShotInput)){
			TakeScreenshot();
		}
	}

	#region private functions

	void StartCapture(){
		m_isRecording = true;
		m_frameCount = 0;
		if (m_lockFrameRate){
			Application.targetFrameRate = m_frameLock;
		}
	}

	void StopCapture(){
		m_isRecording = false;
		Application.targetFrameRate = -1;
	}

	void TakeScreenshot(){
		Application.CaptureScreenshot(m_screenshotsSubfolder + "/" + m_screenShotPrefix + "_" + System.DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".png");
	}

	void CaptureFrame(){
		Application.CaptureScreenshot(m_videoframeSubfolder + "/" + (m_frameCount++) + ".png"); 
	}

	#endregion
}
