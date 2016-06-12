using UnityEngine;
using System.Collections;
using MidiJack;

[ExecuteInEditMode]
public class MIDIInputManager : MonoBehaviour {

	public static MIDIInputManager instance;

	private int[] m_playerKnobNumbers 	= {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07};
	private int m_playButton 			= 0x29;
	private int m_stopButton 			= 0x2A;
	private int m_rewindButton			= 0x2B;
	private int m_forwardButton 		= 0x2C;

	private bool m_playButtonDown		= false;
	private bool m_stopButtonDown		= false;
	private bool m_rewindButtonDown		= false;
	private bool m_forwardButtonDown	= false;

	private float[] m_faderPosition		= new float[Constants.NUMBER_OF_PLAYERS];	// internal position of the faders
	private float[] m_faderPositionMidi	= new float[Constants.NUMBER_OF_PLAYERS];	// current position of the midi hardware faders

	// TODO: implement calibration
	private bool m_isCalibrating		= false;

	// Use this for initialization
	void Awake () {
		if (!instance){
			instance = this;
		} else {
			Destroy(this);
		}
	}
	
	// Update is called once per frame
	void Update () {
		CheckButtonDownStatus();
		UpdatePlayerInput();
	}

	#region private functions

	void CheckButtonDownStatus(){
		if (m_playButtonDown){
			if (MidiMaster.GetKnob(GetPlayButtonKnobNumber()) < 1.0f){
				m_playButtonDown = false;
			}
		}
		if (m_stopButtonDown){
			if (MidiMaster.GetKnob(GetStopButtonKnobNumber()) < 1.0f){
				m_stopButtonDown = false;
			}
		}
		if (m_rewindButtonDown){
			if (MidiMaster.GetKnob(GetRewindButtonKnobNumber()) < 1.0f){
				m_rewindButtonDown = false;
			}
		}
		if (m_forwardButtonDown){
			if (MidiMaster.GetKnob(GetForwardButtonKnobNumber()) < 1.0f){
				m_forwardButtonDown = false;
			}
		}
	}

	void UpdatePlayerInput(){
		for (int i = 0; i < m_faderPosition.Length; i++){
			if (m_faderPositionMidi[i] == MidiMaster.GetKnob(m_playerKnobNumbers[i])){
				// fader didn't move, use debug input
				m_faderPosition[i] = Mathf.Clamp(m_faderPosition[i] + (Input.GetAxis("Fader " + i) / 2), 0.0f, 1.0f);
			} else {
				// midi fader movement overrides current position
				m_faderPosition[i]		= MidiMaster.GetKnob(m_playerKnobNumbers[i]);
				m_faderPositionMidi[i]	= MidiMaster.GetKnob(m_playerKnobNumbers[i]); 
			}

		}
	}

	#endregion

	#region public input

	public float GetInputOfPlayer(int _playerId){
		return m_faderPosition[_playerId];
//		return MidiMaster.GetKnob(m_playerKnobNumbers[_playerId]);
	}

	public bool GetPlayButtonDown(){
		if (MidiMaster.GetKnob(GetPlayButtonKnobNumber()) >= 1.0f && !m_playButtonDown){
			m_playButtonDown = true;
			return true;
		}
		return false;
	}

	public bool GetStopButtonDown(){
		if (MidiMaster.GetKnob(GetStopButtonKnobNumber()) >= 1.0f && !m_stopButtonDown){
			m_stopButtonDown = true;
			return true;
		}
		return false;
	}

	public bool GetRewindButtonDown(){
		if (MidiMaster.GetKnob(GetRewindButtonKnobNumber()) >= 1.0f && !m_rewindButtonDown){
			m_rewindButtonDown = true;
			return true;
		}
		return false;
	}

	public bool GetForwardButtonDown(){
		if (MidiMaster.GetKnob(GetForwardButtonKnobNumber()) >= 1.0f && !m_forwardButtonDown){
			m_forwardButtonDown = true;
			return true;
		}
		return false;
	}

	#endregion

	#region public setter

	public void SetPlayerKnobNumber(int _playerId, int _knobNumber){
		m_playerKnobNumbers[_playerId] = _knobNumber;
	}	

	public void SetPlayButton(int _knobNumber){
		m_playButton = _knobNumber;
	}

	public void SetStopButton(int _knobNumber){
		m_stopButton = _knobNumber;
	}

	public void SetRewindButton(int _knobNumber){
		m_rewindButton = _knobNumber;
	}

	public void SetForwardButton(int _knobNumber){
		m_forwardButton = _knobNumber;
	}

	#endregion

	#region public getter

	public int GetPlayerKnobNumber(int _playerId){
		if (_playerId >= m_playerKnobNumbers.Length) return -1;
		return m_playerKnobNumbers[_playerId];
	}

	public int GetPlayButtonKnobNumber(){
		return m_playButton;
	}

	public int GetStopButtonKnobNumber(){
		return m_stopButton;
	}

	public int GetRewindButtonKnobNumber(){
		return m_rewindButton;
	}

	public int GetForwardButtonKnobNumber(){
		return m_forwardButton;
	}

	public bool IsCalibrating(){
		return m_isCalibrating;
	}

	#endregion
}
