using UnityEngine;
using System.Collections;

public class MovementAnchor : MonoBehaviour {

	public Vector3 m_pointOfOrigin;
	[Range(1,16)]
	public int m_unitsPerBeat;

	private Vector3 m_direction = new Vector3(1.0f, 0, 0);

	private AudioManager m_audioManager;

	// Use this for initialization
	void Awake () {
		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (m_audioManager == null){
			Debug.LogError("no AudioManager was found in this scene");
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = m_pointOfOrigin + m_direction * m_unitsPerBeat * m_audioManager.GetCurrentBeatTime();
	}
}
