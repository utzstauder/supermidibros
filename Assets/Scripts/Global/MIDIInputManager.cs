using UnityEngine;
using System.Collections;
using MidiJack;

[ExecuteInEditMode]
public class MIDIInputManager : MonoBehaviour {

	public static MIDIInputManager instance;

	private Enums.ControlScheme controlScheme = Enums.ControlScheme.MidiController;

	private int[] m_playerFaderNumbers 	= {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07};
	private int[] m_playerKnobNumbers 	= {0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F};
	private int[] m_playerButtonNumbers	= {0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17};
	private bool[] m_playerButtonDown 	= {false, false, false, false, false, false, false, false};
	private int m_playButton 			= 0x29;
	private int m_stopButton 			= 0x2A;
	private int m_rewindButton			= 0x2B;
	private int m_forwardButton 		= 0x2C;

	private bool m_playButtonDown		= false;
	private bool m_stopButtonDown		= false;
	private bool m_rewindButtonDown		= false;
	private bool m_forwardButtonDown	= false;

	private float[] m_faderPosition		= new float[Constants.NUMBER_OF_PLAYERS];	// internal position of the faders
	private float[] m_faderPositionPrev	= new float[Constants.NUMBER_OF_PLAYERS];
	private float[] m_faderPositionMidi	= new float[Constants.NUMBER_OF_PLAYERS];	// current position of the midi hardware faders
	private float[] m_knobPositionMidi	= new float[Constants.NUMBER_OF_PLAYERS];
	private float[] m_knobPositionMidiPrev	= new float[Constants.NUMBER_OF_PLAYERS];
	private bool sendUpdateEvent = false;

	private float idleTimer = 0.0f;
	[SerializeField]
	private float timeUntilIdle = 10.0f;

	public delegate void MidiInputEvent(float[] values);
	public static event MidiInputEvent OnKnobInput;

	public delegate void MidiButtonEvent(bool[] pressedButtons);
	public static event MidiButtonEvent OnButtonInput;

	// TODO: implement calibration
	private bool m_isCalibrating		= false;

	// Use this for initialization
	void Awake () {
		if (Application.isPlaying){
			if (!instance){
				instance = this;
			} else {
				Destroy(this);
			}
		}

		int numberOfJoysticks = Input.GetJoystickNames().Length;
		Debug.Log(Input.GetJoystickNames().Length + " joystick(s) connected");
		for (int i = 0; i < numberOfJoysticks; i++){
			Debug.Log("Joystick 1: " + Input.GetJoystickNames()[i]);
		}


	}
	
	// Update is called once per frame
	void Update () {
		ResetButtonDownStatus();
		UpdatePlayerInput();
		//Debug.Log(Input.GetAxis("Fader " + 0));

		if (Input.GetKeyDown(KeyCode.Alpha1)){
			controlScheme = Enums.ControlScheme.Keyboard;
		} else if (Input.GetKeyDown(KeyCode.Alpha2)){
			controlScheme = Enums.ControlScheme.TwoControllers;
		} else if (Input.GetKeyDown(KeyCode.Alpha4)){
			controlScheme = Enums.ControlScheme.FourControllers;
		} else if (Input.GetKeyDown(KeyCode.Alpha8)){
			controlScheme = Enums.ControlScheme.MidiController;
		}

		idleTimer += Time.deltaTime;
		Debug.Log(idleTimer);
	}

	void Start(){
		if (OnKnobInput != null){
			OnKnobInput(m_knobPositionMidi);
		}
	}

	#region private functions

	void ResetButtonDownStatus(){
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

		for (int i = 0; i < Constants.NUMBER_OF_PLAYERS; i++){
			if (m_playerButtonDown[i]){
				if (MidiMaster.GetKnob(m_playerButtonNumbers[i]) < 1.0f){
					m_playerButtonDown[i] = false;
				}
			}
		}
	}

	void UpdatePlayerInput(){
		sendUpdateEvent = false;

		for (int i = 0; i < m_faderPosition.Length; i++){
			switch(controlScheme){
			case Enums.ControlScheme.MidiController:
				m_faderPosition[i]		= MidiMaster.GetKnob(m_playerFaderNumbers[i]);
				m_faderPositionMidi[i]	= MidiMaster.GetKnob(m_playerFaderNumbers[i]); 
				break;

			case Enums.ControlScheme.Keyboard:
				if (Input.GetButtonDown("Up " + i)){
					m_faderPosition[i] += 1f/(Constants.VERTICAL_POSITIONS-1);
				} else if (Input.GetButtonDown("Down " + i)){
					m_faderPosition[i] -= 1f/(Constants.VERTICAL_POSITIONS-1);
				}

				if (Input.GetButtonDown("ResetPositions")){
					m_faderPosition[i] = .5f;
				}

				m_faderPosition[i] = Mathf.Clamp01(m_faderPosition[i]);
				break;

			case Enums.ControlScheme.TwoControllers:
				if (i < 4){
					m_faderPosition[i] = ((Input.GetAxis("Controller " + i/2 + " " + i%2) * -1) + 1) / 2;
				} else {
					m_faderPosition[i] = ((Input.GetAxis("Controller " + (m_faderPosition.Length - 1 - i)/2 + " " + (i+1)%2) * -1) + 1) / 2;
				}
				break;

			case Enums.ControlScheme.FourControllers:
				m_faderPosition[i] = ((Input.GetAxis("Controller " + i/2 + " " + i%2) * -1) + 1) / 2;
				break;

			default:
				break;
			}

//			if (m_faderPositionMidi[i] == MidiMaster.GetKnob(m_playerFaderNumbers[i])){
//				// fader didn't move, use debug input
//				if (numberOfJoysticks >= 2){
//					// 2 (or more) controllers
//					m_faderPosition[i] = ((Input.GetAxis("Fader " + i) * -1) + 1) / 2;
//				} else if (numberOfJoysticks == 1){
//					// 1 controller
//					m_faderPosition[i] = ((Input.GetAxis("Fader " + i) * -1) + 1) / 2;
//				} else {
//					m_faderPosition[i] = Mathf.Clamp(m_faderPosition[i] + (Input.GetAxis("Fader " + i) / 2), 0.0f, 1.0f);
//				}
//			} else {
//				// midi fader movement overrides current position
//				m_faderPosition[i]		= MidiMaster.GetKnob(m_playerFaderNumbers[i]);
//				m_faderPositionMidi[i]	= MidiMaster.GetKnob(m_playerFaderNumbers[i]); 
//			}

			m_knobPositionMidi[i] = MidiMaster.GetKnob(m_playerKnobNumbers[i]);
			if (m_knobPositionMidi[i] < m_knobPositionMidiPrev[i] ||
				m_knobPositionMidi[i] > m_knobPositionMidiPrev[i]){
				sendUpdateEvent = true;
			}
			m_knobPositionMidiPrev[i] = m_knobPositionMidi[i];

			if (m_faderPosition[i] != m_faderPositionPrev[i]){
				idleTimer = 0;
			}
			m_faderPositionPrev[i] = m_faderPosition[i];

		}

		if (sendUpdateEvent){
			if (OnKnobInput != null){
				OnKnobInput(m_knobPositionMidi);
			}
		}
	}

	#endregion

	#region public input

	public float GetInputOfPlayer(int _playerId){
		return m_faderPosition[_playerId];
//		return MidiMaster.GetKnob(m_playerKnobNumbers[_playerId]);
	}

	public float GetKnobInputOfPlayer(int _playerId){
		return m_knobPositionMidi[_playerId];
	}

	public bool GetPlayerButtonDown(int _playerId){
		if (MidiMaster.GetKnob(m_playerButtonNumbers[_playerId]) >= 1.0f && !m_playerButtonDown[_playerId]){
			m_playerButtonDown[_playerId] = true;
			return true;
		}
		return false;
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
		m_playerFaderNumbers[_playerId] = _knobNumber;
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
		if (_playerId >= m_playerFaderNumbers.Length) return -1;
		return m_playerFaderNumbers[_playerId];
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

	public bool IsIdle(){
		return (idleTimer > timeUntilIdle);
	}

	#endregion
}
