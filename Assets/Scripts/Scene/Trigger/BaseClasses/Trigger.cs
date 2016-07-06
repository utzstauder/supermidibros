using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Trigger : MonoBehaviour {
	protected bool m_inEditMode = true;

	public delegate void TriggerDelegate(Trigger scriptReference, bool success);
	public event TriggerDelegate OnTrigger;

	protected AudioManager m_audioManager;

	private Color m_gizmosColor = Color.yellow;

	// Use this for initialization
	protected virtual void Awake () {
		if (Application.isPlaying){
			m_inEditMode = false;
		} else {
			m_inEditMode = true;
		}

		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (m_audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {
			m_audioManager.OnStop += OnStop;
		}
	}

	protected virtual void Update(){
		
	}

	protected virtual void OnDestroy(){
		OnTrigger = null;
	}

	/**
	 * This will check if every child has been triggered
	 */ 
	protected virtual void OnReceiveTrigger(Trigger scriptReference, bool success){
		
	}


	/**
	 * This function will be called everytime the player stops audio playback
	 * used for debugging only
	 */
	protected virtual void OnStop(){
		
	}


	#region public functions

	/**
	 * This will send a trigger event to every target this trigger is attached to
	 */
	protected virtual void BroadcastTriggerSuccess(){
		//Debug.Log("Trigger Success");
		if (OnTrigger != null) {
			OnTrigger(this, true);
		}
	}

	protected virtual void BroadcastTriggerFailure(){
		//Debug.Log("Trigger Failure");
		if (OnTrigger != null) {
			OnTrigger(this, false);
		}
	}

	/**
	 * This gets called whenever a player object collides with this trigger
	 */
	public virtual void OnCollision(int _playerId){
//		Debug.Log("Collision with Player " + _playerId);
		//Debug.Log("HIT");
		BroadcastTriggerSuccess();
	}

	public virtual void OnMiss (int _playerId){
		//Debug.Log("MISS");
		BroadcastTriggerFailure();
	}

	#endregion


	#region gizmos

	protected virtual void OnDrawGizmos(){
		Gizmos.color = m_gizmosColor;
	}

	#endregion
}
